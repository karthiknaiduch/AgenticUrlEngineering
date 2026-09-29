namespace AgenticEngineering.Domain.Models;

public class PolicyDecision
{
    public bool Allowed { get; set; }

    public bool RequiresApproval { get; set; }

    public string Reason { get; set; } = string.Empty;

    public List<string> Violations { get; set; } = [];
}