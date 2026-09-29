using AgenticEngineering.Application.Agents;
using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Orchestration;

public class WorkflowExecutor
{
    private readonly IReadOnlyDictionary<string, IEngineeringAgent> _agents;
    private readonly IPolicyEngine _policyEngine;
    private readonly IRollbackHandler _rollbackHandler;

    public WorkflowExecutor(
        IEnumerable<IEngineeringAgent> agents,
        IPolicyEngine policyEngine,
        IRollbackHandler rollbackHandler)
    {
        _agents = agents.ToDictionary(
            x => x.AgentType,
            StringComparer.OrdinalIgnoreCase);

        _policyEngine = policyEngine;
        _rollbackHandler = rollbackHandler;
    }

    public async Task<AgentResult> ExecuteAsync(
        Workflow workflow,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        workflow.Status = WorkflowStatus.Running;
        workflow.UpdatedAtUtc = DateTime.UtcNow;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pendingNodes = workflow.Nodes
                .Where(x => x.Status == NodeStatus.Pending)
                .ToList();

            // No pending nodes means the workflow has finished.
            if (pendingNodes.Count == 0)
            {
                workflow.Status = WorkflowStatus.Completed;
                workflow.UpdatedAtUtc = DateTime.UtcNow;

                return new AgentResult
                {
                    Status = AgentExecutionStatus.Succeeded,
                    Message = "Workflow completed successfully."
                };
            }

            // Find nodes whose dependencies have all completed.
            var readyNodes = pendingNodes
                .Where(node =>
                    node.DependsOn.All(dependencyId =>
                        workflow.Nodes.Any(x =>
                            x.Id == dependencyId &&
                            x.Status == NodeStatus.Completed)))
                .ToList();

            // If we have pending nodes but nothing is executable,
            // the workflow is blocked.
            if (readyNodes.Count == 0)
            {
                workflow.Status = WorkflowStatus.Blocked;
                workflow.UpdatedAtUtc = DateTime.UtcNow;

                return new AgentResult
                {
                    Status = AgentExecutionStatus.Blocked,
                    Message =
                        "Workflow is blocked because no pending nodes are executable.",
                    Errors =
                    [
                        "Check workflow dependencies and node statuses."
                    ]
                };
            }

            // Execute independent nodes in parallel.
            var tasks = readyNodes.Select(node =>
                ExecuteNodeAsync(
                    workflow,
                    node,
                    context,
                    cancellationToken));

            var results = await Task.WhenAll(tasks);

            // If any node requires human approval, pause the workflow.
            var approvalRequired = results.Any(
                x => x.Status == AgentExecutionStatus.WaitingForApproval);

            if (approvalRequired)
            {
                workflow.Status = WorkflowStatus.WaitingForApproval;
                workflow.UpdatedAtUtc = DateTime.UtcNow;

                return new AgentResult
                {
                    Status = AgentExecutionStatus.WaitingForApproval,
                    Message =
                        "Workflow is waiting for human approval."
                };
            }

            // If any node was blocked, stop the workflow.
            var blocked = results.FirstOrDefault(
                x => x.Status == AgentExecutionStatus.Blocked);

            if (blocked != null)
            {
                workflow.Status = WorkflowStatus.Blocked;
                workflow.UpdatedAtUtc = DateTime.UtcNow;

                return blocked;
            }

            // If a policy violation or safe-stop occurred,
            // do not retry automatically.
            var safeStopped = results.FirstOrDefault(
                x => x.Status == AgentExecutionStatus.SafeStopped);

            if (safeStopped != null)
            {
                workflow.Status = WorkflowStatus.SafeStopped;
                workflow.UpdatedAtUtc = DateTime.UtcNow;

                return safeStopped;
            }

            // If any node permanently failed, execute rollback.
            var failed = results.FirstOrDefault(
                x => x.Status == AgentExecutionStatus.Failed);

            if (failed != null)
            {
                await _rollbackHandler.RollbackAsync(
                    workflow,
                    context,
                    cancellationToken);

                return new AgentResult
                {
                    Status = AgentExecutionStatus.Failed,
                    Message =
                        "Workflow failed and rollback was executed.",
                    FailureType = failed.FailureType,
                    Errors = results
                        .Where(x =>
                            x.Status == AgentExecutionStatus.Failed)
                        .SelectMany(x => x.Errors)
                        .ToList()
                };
            }

            // At this point all ready nodes succeeded.
            // The loop continues and evaluates the next DAG level.
        }
    }

    private async Task<AgentResult> ExecuteNodeAsync(
        Workflow workflow,
        WorkflowNode node,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------
        // 1. Resolve the agent
        // ---------------------------------------------------------

        if (!_agents.TryGetValue(
                node.AgentType,
                out var agent))
        {
            node.Status = NodeStatus.Failed;

            return new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.Validation,
                Message =
                    $"Agent '{node.AgentType}' was not registered.",
                Errors =
                [
                    $"No implementation found for agent '{node.AgentType}'."
                ]
            };
        }

        // ---------------------------------------------------------
        // 2. Evaluate governance policy
        // ---------------------------------------------------------

        var policyDecision = _policyEngine.Evaluate(
            workflow,
            node,
            context);

        if (!policyDecision.Allowed)
        {
            node.Status = NodeStatus.SafeStopped;

            return new AgentResult
            {
                Status = AgentExecutionStatus.SafeStopped,
                FailureType = FailureType.PolicyViolation,
                Message =
                    "Workflow execution blocked by policy.",
                Errors = policyDecision.Violations
            };
        }

        // ---------------------------------------------------------
        // 3. Human approval gate
        // ---------------------------------------------------------

        if (policyDecision.RequiresApproval &&
            node.ApprovalStatus != ApprovalStatus.Approved)
        {
            node.RequiresApproval = true;
            node.Status = NodeStatus.WaitingForApproval;

            workflow.Status = WorkflowStatus.WaitingForApproval;

            return new AgentResult
            {
                Status = AgentExecutionStatus.WaitingForApproval,
                Message =
                    $"Human approval required: {policyDecision.Reason}"
            };
        }

        // ---------------------------------------------------------
        // 4. Execute with bounded retries
        // ---------------------------------------------------------

        while (node.RetryCount <= node.MaxRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            node.Status = NodeStatus.Running;
            workflow.UpdatedAtUtc = DateTime.UtcNow;

            try
            {
                var result = await agent.ExecuteAsync(
                    context,
                    cancellationToken);

                // -------------------------------------------------
                // Successful execution
                // -------------------------------------------------

                if (result.Status == AgentExecutionStatus.Succeeded)
                {
                    node.Status = NodeStatus.Completed;

                    MergeResultIntoContext(
                        context,
                        result);

                    return result;
                }

                // -------------------------------------------------
                // Human approval requested by the agent itself
                // -------------------------------------------------

                if (result.Status ==
                    AgentExecutionStatus.WaitingForApproval)
                {
                    node.Status = NodeStatus.WaitingForApproval;
                    workflow.Status = WorkflowStatus.WaitingForApproval;

                    return result;
                }

                // -------------------------------------------------
                // Policy/safe-stop result
                // -------------------------------------------------

                if (result.Status ==
                    AgentExecutionStatus.SafeStopped)
                {
                    node.Status = NodeStatus.SafeStopped;

                    return result;
                }

                // -------------------------------------------------
                // Blocked result
                // -------------------------------------------------

                if (result.Status ==
                    AgentExecutionStatus.Blocked)
                {
                    node.Status = NodeStatus.Failed;

                    return result;
                }

                // -------------------------------------------------
                // Failed result
                // -------------------------------------------------

                context.PreviousFailures.Add(
                    $"{node.AgentType}: {result.Message}");

                // Do not retry failures that are not retryable.
                if (!ShouldRetry(result.FailureType))
                {
                    node.Status = NodeStatus.Failed;

                    return result;
                }

                // Retryable failure.
                node.RetryCount++;

                if (node.RetryCount > node.MaxRetries)
                {
                    node.Status = NodeStatus.Failed;

                    return new AgentResult
                    {
                        Status = AgentExecutionStatus.Failed,
                        FailureType = result.FailureType,
                        Message =
                            $"Agent '{node.AgentType}' failed after " +
                            $"{node.MaxRetries} retries.",
                        Errors = result.Errors
                    };
                }

                node.Status = NodeStatus.Retrying;

                await DelayBeforeRetryAsync(
                    node.RetryCount,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Unexpected exceptions are treated as Unknown,
                // which is currently retryable.
                context.PreviousFailures.Add(
                    $"{node.AgentType}: {ex.Message}");

                node.RetryCount++;

                if (node.RetryCount > node.MaxRetries)
                {
                    node.Status = NodeStatus.Failed;

                    return new AgentResult
                    {
                        Status = AgentExecutionStatus.Failed,
                        FailureType = FailureType.Unknown,
                        Message =
                            $"Agent '{node.AgentType}' failed " +
                            $"after {node.MaxRetries} retries.",
                        Errors =
                        [
                            ex.Message
                        ]
                    };
                }

                node.Status = NodeStatus.Retrying;

                await DelayBeforeRetryAsync(
                    node.RetryCount,
                    cancellationToken);
            }
        }

        // Defensive fallback.
        node.Status = NodeStatus.Failed;

        return new AgentResult
        {
            Status = AgentExecutionStatus.Failed,
            FailureType = FailureType.Unknown,
            Message =
                $"Agent '{node.AgentType}' failed unexpectedly."
        };
    }

    private static void MergeResultIntoContext(
        AgentContext context,
        AgentResult result)
    {
        foreach (var artifact in result.Artifacts)
        {
            context.Artifacts.Add(artifact);
        }

        foreach (var data in result.Data)
        {
            context.Data[data.Key] = data.Value;
        }
    }

    private static async Task DelayBeforeRetryAsync(
        int retryCount,
        CancellationToken cancellationToken)
    {
        // Simple exponential backoff:
        // retry 1 -> 1 second
        // retry 2 -> 2 seconds
        // retry 3 -> 4 seconds
        var delaySeconds = Math.Pow(2, retryCount - 1);

        await Task.Delay(
            TimeSpan.FromSeconds(delaySeconds),
            cancellationToken);
    }

    private static bool ShouldRetry(
        FailureType failureType)
    {
        return failureType switch
        {
            FailureType.Transient => true,
            FailureType.Unknown => true,
            FailureType.ImplementationFailure => true,

            FailureType.Validation => false,
            FailureType.PolicyViolation => false,
            FailureType.SecurityFailure => false,
            FailureType.DependencyFailure => false,

            _ => false
        };
    }
}