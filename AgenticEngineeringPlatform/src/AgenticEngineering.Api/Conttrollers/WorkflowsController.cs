using AgenticEngineering.Api.Models;
using AgenticEngineering.Application.Models;
using AgenticEngineering.Application.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace AgenticEngineering.Api.Controllers;

[ApiController]
[Route("api/v1/workflows")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowOrchestrator _orchestrator;

    public WorkflowsController(
        IWorkflowOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet("{workflowId:guid}")]
public async Task<IActionResult> GetWorkflow(
    Guid workflowId,
    CancellationToken cancellationToken)
{
    // The store is responsible for retrieving the persisted
    // workflow execution state.
    var workflowStore =
        HttpContext.RequestServices
            .GetRequiredService<IWorkflowStore>();

    var stored = await workflowStore.GetAsync(
        workflowId,
        cancellationToken);

    if (stored == null)
    {
        return NotFound(new
        {
            workflowId,
            message = "Workflow was not found."
        });
    }

    var workflow = stored.Value.Workflow;
    var context = stored.Value.Context;

    return Ok(new
    {
        workflowId = workflow.Id,
        workflow.Requirement,
        status = workflow.Status.ToString(),
        version = workflow.Version,
        createdAtUtc = workflow.CreatedAtUtc,
        updatedAtUtc = workflow.UpdatedAtUtc,

        nodes = workflow.Nodes.Select(node => new
        {
            nodeId = node.Id,
            agentType = node.AgentType,
            status = node.Status.ToString(),
            dependsOn = node.DependsOn,
            retryCount = node.RetryCount,
            maxRetries = node.MaxRetries,
            requiresApproval = node.RequiresApproval,
            approvalStatus = node.ApprovalStatus.ToString(),
            approvalId = node.ApprovalId
        }),

        artifacts = context.Artifacts,

        previousFailures = context.PreviousFailures,

        approvals = context.ApprovalRecords.Select(approval => new
        {
            approvalId = approval.Id,
            workflowId = approval.WorkflowId,
            nodeId = approval.NodeId,
            requestedBy = approval.RequestedBy,
            approvedBy = approval.ApprovedBy,
            status = approval.Status.ToString(),
            reason = approval.Reason,
            requestedAtUtc = approval.RequestedAtUtc,
            decisionAtUtc = approval.DecisionAtUtc
        }),

        decisions = context.Decisions.Select(decision => new
        {
            decisionId = decision.Id,
            decisionText = decision.DecisionText,
            reason = decision.Reason,
            evidence = decision.Evidence,
            alternatives = decision.Alternatives,
            selectedOption = decision.SelectedOption,
            actor = decision.Actor,
            timestampUtc = decision.TimestampUtc
        })
    });
}

    [HttpPost]
    public async Task<IActionResult> CreateWorkflow(
        [FromBody] CreateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Requirement))
        {
            return BadRequest(new
            {
                message = "Requirement is required."
            });
        }

        var context = new AgentContext
        {
            WorkflowId = Guid.NewGuid(),
            Requirement = request.Requirement
        };

        var result = await _orchestrator.ExecuteAsync(
            context,
            cancellationToken);

        return result.Status switch
        {
            AgentExecutionStatus.Succeeded =>
                Ok(new
                {
                    workflowId = context.WorkflowId,
                    status = result.Status.ToString(),
                    message = result.Message,
                    artifacts = context.Artifacts,
                    data = context.Data
                }),

            AgentExecutionStatus.WaitingForApproval =>
                StatusCode(StatusCodes.Status202Accepted, new
                {
                    workflowId = context.WorkflowId,
                    status = result.Status.ToString(),
                    message = result.Message
                }),

            AgentExecutionStatus.Blocked =>
                StatusCode(StatusCodes.Status423Locked, new
                {
                    workflowId = context.WorkflowId,
                    status = result.Status.ToString(),
                    message = result.Message
                }),

            AgentExecutionStatus.SafeStopped =>
                StatusCode(StatusCodes.Status403Forbidden, new
                {
                    workflowId = context.WorkflowId,
                    status = result.Status.ToString(),
                    message = result.Message,
                    errors = result.Errors
                }),

            _ =>
                StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    workflowId = context.WorkflowId,
                    status = result.Status.ToString(),
                    message = result.Message,
                    errors = result.Errors
                })
        };
    }
}