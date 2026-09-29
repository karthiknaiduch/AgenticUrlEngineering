using AgenticEngineering.Application.Models;

namespace AgenticEngineering.Application.Agents;

public interface IEngineeringAgent
{
    string AgentType { get; }

    Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken);
}