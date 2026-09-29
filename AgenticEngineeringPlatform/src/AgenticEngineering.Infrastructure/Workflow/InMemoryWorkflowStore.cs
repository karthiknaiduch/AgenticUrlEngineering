using System.Collections.Concurrent;
using AgenticEngineering.Application.Models;
using AgenticEngineering.Application.Orchestration;
using AgenticEngineering.Domain.Entities;

namespace AgenticEngineering.Infrastructure.Workflow;

public class InMemoryWorkflowStore : IWorkflowStore
{
    private readonly ConcurrentDictionary<
        Guid,
        (AgenticEngineering.Domain.Entities.Workflow Workflow,
         AgentContext Context)> _workflows = new();

    public Task SaveAsync(
        AgenticEngineering.Domain.Entities.Workflow workflow,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _workflows[workflow.Id] = (workflow, context);

        return Task.CompletedTask;
    }

    public Task<(
        AgenticEngineering.Domain.Entities.Workflow Workflow,
        AgentContext Context)?> GetAsync(
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_workflows.TryGetValue(
                workflowId,
                out var workflowContext))
        {
            return Task.FromResult<
                (AgenticEngineering.Domain.Entities.Workflow Workflow,
                 AgentContext Context)?>(
                workflowContext);
        }

        return Task.FromResult<
            (AgenticEngineering.Domain.Entities.Workflow Workflow,
             AgentContext Context)?>(
            null);
    }
}