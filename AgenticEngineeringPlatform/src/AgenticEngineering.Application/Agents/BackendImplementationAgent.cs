using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class BackendImplementationAgent : IEngineeringAgent
{
    public string AgentType => "BackendImplementationAgent";

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
                    "TaskPlannerAgent must complete before backend implementation."
                ]
            });
        }

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Backend implementation plan prepared.",
            Artifacts =
            [
                "backend-implementation.md"
            ],
            Data =
            {
                ["BackendImplementation"] = new
                {
                    Framework = "ASP.NET Core",
                    ApiStyle = "REST",
                    Components = new[]
                    {
                        "Controllers",
                        "Application Services",
                        "Domain Logic",
                        "Persistence Layer"
                    }
                }
            }
        });
    }
}