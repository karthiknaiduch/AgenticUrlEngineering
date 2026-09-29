using AgenticEngineering.Application.Models;

namespace AgenticEngineering.Application.Orchestration;

public interface IWorkflowOrchestrator
{
    Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken);

    Task<AgentResult> ResumeAsync(
        Guid workflowId,
        string approvedBy,
        string reason,
        CancellationToken cancellationToken);
}