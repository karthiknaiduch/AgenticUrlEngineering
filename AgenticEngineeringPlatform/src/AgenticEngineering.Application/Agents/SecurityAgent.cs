using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class SecurityAgent : IEngineeringAgent
{
    public string AgentType => "SecurityAgent";

    public Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!context.Data.ContainsKey("TestingResult"))
        {
            return Task.FromResult(new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.DependencyFailure,
                Message = "Testing must complete before security validation.",
                Errors =
                [
                    "TestingAgent result is required."
                ]
            });
        }

        // Controlled failure used to verify rollback behavior.
        if (context.Requirement.Contains(
                "force security failure",
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.SecurityFailure,
                Message =
                    "Simulated security failure for rollback testing.",
                Errors =
                [
                    "Security validation failed intentionally for rollback testing."
                ]
            });
        }

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Security validation completed.",
            Artifacts =
            [
                "security-review.md"
            ],
            Data =
            {
                ["SecurityResult"] = new
                {
                    Authentication = "Review required",
                    Authorization = "Review required",
                    InputValidation = "Review required",
                    Secrets = "Externalized configuration required",
                    Dependencies = "Dependency vulnerability scan required"
                }
            }
        });
    }
}