using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class DatabaseAgent : IEngineeringAgent
{
    public string AgentType => "DatabaseAgent";

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
                    "TaskPlannerAgent must complete before database implementation."
                ]
            });
        }

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Database implementation plan prepared.",
            Artifacts =
            [
                "database-implementation.md"
            ],
            Data =
            {
                ["DatabaseImplementation"] = new
                {
                    Database = "PostgreSQL",
                    Components = new[]
                    {
                        "Schema",
                        "Migrations",
                        "Indexes",
                        "Repository/Data Access Layer"
                    }
                }
            }
        });
    }
}