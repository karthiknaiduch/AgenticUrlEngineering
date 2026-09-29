using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class FrontendImplementationAgent : IEngineeringAgent
{
    public string AgentType => "FrontendImplementationAgent";

    public Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!context.Data.ContainsKey("ImplementationPlan"))
        {
            return Task.FromResult(new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.Validation,
                Message = "Implementation plan is required.",
                Errors =
                [
                    "TaskPlannerAgent must complete before frontend implementation."
                ]
            });
        }

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Frontend implementation plan prepared.",
            Artifacts =
            [
                "frontend-implementation.md"
            ],
            Data =
            {
                ["FrontendImplementation"] = new
                {
                    Framework = "Angular",
                    Language = "TypeScript",
                    Components = new[]
                    {
                        "Angular Components",
                        "Services",
                        "HttpClient",
                        "Routing"
                    }
                }
            }
        });
    }
}