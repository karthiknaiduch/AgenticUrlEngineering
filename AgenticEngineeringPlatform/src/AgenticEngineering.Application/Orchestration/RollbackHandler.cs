using AgenticEngineering.Application.Models;
using AgenticEngineering.Application.Orchestration;
using AgenticEngineering.Domain.Entities;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Infrastructure.Orchestration;

public class RollbackHandler : IRollbackHandler
{
    public Task RollbackAsync(
        Workflow workflow,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        workflow.Status = WorkflowStatus.RollingBack;

        foreach (var node in workflow.Nodes)
        {
            if (node.Status == NodeStatus.Completed)
            {
                node.Status = NodeStatus.RolledBack;
            }
        }

        workflow.UpdatedAtUtc = DateTime.UtcNow;

        context.PreviousFailures.Add(
            "Workflow rollback executed after unrecoverable failure.");

        workflow.Status = WorkflowStatus.Failed;
        workflow.UpdatedAtUtc = DateTime.UtcNow;

        return Task.CompletedTask;
    }
}