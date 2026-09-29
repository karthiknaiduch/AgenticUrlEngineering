namespace AgenticEngineering.Api.Models;

public class ApprovalRequest
{
    public string ApprovedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}