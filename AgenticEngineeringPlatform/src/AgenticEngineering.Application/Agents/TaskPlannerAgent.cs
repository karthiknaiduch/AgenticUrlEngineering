using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;

namespace AgenticEngineering.Application.Agents;

public class TaskPlannerAgent : IEngineeringAgent
{
    public string AgentType => "TaskPlannerAgent";

    public Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!context.Data.TryGetValue(
                "Architecture",
                out var architecture))
        {
            return Task.FromResult(new AgentResult
            {
                Status = AgentExecutionStatus.Failed,
                FailureType = FailureType.Validation,
                Message = "Architecture is required before task planning.",
                Errors =
                [
                    "ArchitectureAgent must complete before TaskPlannerAgent."
                ]
            });
        }

        var tasks = new
        {
            Backend = new[]
            {
                "Create ASP.NET Core API",
                "Implement business services",
                "Implement REST endpoints",
                "Implement persistence layer"
            },

            Frontend = new[]
            {
                "Create Angular application",
                "Implement API integration",
                "Implement UI components"
            },

            Database = new[]
            {
                "Create database schema",
                "Create migrations",
                "Add indexes",
                "Configure database access"
            },

            Testing = new[]
            {
                "Create unit tests",
                "Create integration tests",
                "Validate API behavior"
            },

            Security = new[]
            {
                "Review authentication",
                "Review authorization",
                "Validate input handling",
                "Review secrets and configuration"
            }
        };

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Engineering tasks planned successfully.",
            Artifacts =
            [
                "implementation-plan.md"
            ],
            Data =
            {
                ["Architecture"] = architecture,
                ["ImplementationPlan"] = tasks
            }
        });
    }
}