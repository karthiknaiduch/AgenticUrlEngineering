namespace AgenticEngineering.Application.Models;

public enum AgentExecutionStatus
{
    Succeeded,
    Failed,
    WaitingForApproval,
    Blocked,
    SafeStopped
}