using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class TestingAgent : IEngineeringAgent
{
    public string AgentType => "TestingAgent";

    public Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requiredArtifacts = new[]
        {
            "BackendImplementation",
            "FrontendImplementation",
            "DatabaseImplementation"
        };

        var missing = requiredArtifacts
            .Where(x => !context.Data.ContainsKey(x))
            .ToList();

        if (missing.Count > 0)
        {
            return Task.FromResult(new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.DependencyFailure,
                Message = "Implementation dependencies are incomplete.",
                Errors = missing
                    .Select(x => $"{x} is missing.")
                    .ToList()
            });
        }

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Testing validation completed.",
            Artifacts =
            [
                "test-plan.md"
            ],
            Data =
            {
                ["TestingResult"] = new
                {
                    UnitTests = "Required",
                    IntegrationTests = "Required",
                    ApiValidation = "Required",
                    RegressionTesting = "Required"
                }
            }
        });
    }
}