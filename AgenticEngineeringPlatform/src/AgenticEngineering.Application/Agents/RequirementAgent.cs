using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class RequirementAgent : IEngineeringAgent
{
    public string AgentType => "RequirementAgent";

    public Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.Requirement))
        {
            return Task.FromResult(new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.Validation,
                Message = "Requirement is empty.",
                Errors =
                [
                    "A valid engineering requirement is required."
                ]
            });
        }

        var normalizedRequirement =
            context.Requirement.Trim();

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Requirement analyzed successfully.",
            Data =
            {
                ["NormalizedRequirement"] =
                    normalizedRequirement
            }
        });
    }
}