namespace AgenticEngineering.Domain.Enums;

public enum NodeStatus
{
    Pending,
    Running,
    WaitingForApproval,
    Completed,
    Failed,
    Retrying,
    Skipped,
    RolledBack,
    SafeStopped
}