using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Enums;
using AgenticEngineering.Domain.Models;

namespace AgenticEngineering.Application.Agents;

public class ArchitectureAgent : IEngineeringAgent
{
    public string AgentType => "ArchitectureAgent";

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
                Message =
                    "Cannot create architecture without a requirement.",
                Errors =
                [
                    "Requirement is required before architecture analysis."
                ]
            });
        }

        var architecture = """
            Proposed architecture:
            - ASP.NET Core API for backend services
            - Angular for the web client
            - REST APIs for synchronous communication
            - PostgreSQL for relational persistence
            - Redis for caching where appropriate
            - Kafka or a messaging platform for asynchronous processing
            - Containerized deployment
            - Automated unit and integration testing
            - Centralized logging and observability
            """;

        context.Decisions.Add(new Decision
        {
            Id = Guid.NewGuid(),
            WorkflowId = context.WorkflowId,
            DecisionText =
                "Select a service-oriented REST architecture.",
            Reason =
                "The requirement requires independently deployable backend capabilities with synchronous API access.",
            Evidence =
            [
                "Requirement specifies API-based transaction processing.",
                "Backend and frontend need a clear service boundary."
            ],
            Alternatives =
            [
                "Monolithic application",
                "Service-oriented architecture",
                "Event-driven architecture"
            ],
            SelectedOption =
                "Service-oriented REST architecture",
            Actor = AgentType,
            TimestampUtc = DateTime.UtcNow
        });

        return Task.FromResult(new AgentResult
        {
            Status = AgentExecutionStatus.Succeeded,
            Message = "Initial architecture created.",
            Artifacts =
            [
                "architecture.md"
            ],
            Data =
            {
                ["Architecture"] = architecture
            }
        });
    }
}