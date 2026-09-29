using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;

namespace AgenticEngineering.Application.Orchestration;

public interface IRollbackHandler
{
    Task RollbackAsync(
        Workflow workflow,
        AgentContext context,
        CancellationToken cancellationToken);
}