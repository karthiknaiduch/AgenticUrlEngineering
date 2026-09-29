using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;
using AgenticEngineering.Domain.Models;

namespace AgenticEngineering.Application.Orchestration;

public class PolicyEngine : IPolicyEngine
{
    public PolicyDecision Evaluate(
        Workflow workflow,
        WorkflowNode node,
        AgentContext context)
    {
        var decision = new PolicyDecision
        {
            Allowed = true
        };

        if (node.AgentType.Equals(
                "ReleaseAgent",
                StringComparison.OrdinalIgnoreCase))
        {
            decision.RequiresApproval = true;
            decision.Reason =
                "Production release requires human approval.";
        }

        if (node.AgentType.Equals(
                "DatabaseAgent",
                StringComparison.OrdinalIgnoreCase))
        {
            decision.RequiresApproval = true;
            decision.Reason =
                "Database changes require human approval.";
        }

        if (context.Requirement.Contains(
                "delete production database",
                StringComparison.OrdinalIgnoreCase))
        {
            decision.Allowed = false;

            decision.Reason =
                "Production database deletion is prohibited.";

            decision.Violations =
            [
                "Destructive production database operation detected."
            ];
        }

        return decision;
    }
}