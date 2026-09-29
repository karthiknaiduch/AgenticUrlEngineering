using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;

namespace AgenticEngineering.Application.Orchestration;

public interface IWorkflowStore
{
    Task SaveAsync(
        Workflow workflow,
        AgentContext context,
        CancellationToken cancellationToken);

    Task<(Workflow Workflow, AgentContext Context)?> GetAsync(
        Guid workflowId,
        CancellationToken cancellationToken);
}