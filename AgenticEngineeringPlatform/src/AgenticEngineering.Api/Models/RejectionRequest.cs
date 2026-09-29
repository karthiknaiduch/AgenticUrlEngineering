namespace AgenticEngineering.Api.Models;

public class RejectionRequest
{
    public string RejectedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}