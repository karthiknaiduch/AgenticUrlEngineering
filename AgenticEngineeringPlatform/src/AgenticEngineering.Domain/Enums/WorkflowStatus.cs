namespace AgenticEngineering.Domain.Enums;

public enum WorkflowStatus
{
    Created,
    Planning,
    Running,
    WaitingForApproval,
    Blocked,
    Retrying,
    Failed,
    RollingBack,
    Completed,
    Cancelled,
    SafeStopped
}