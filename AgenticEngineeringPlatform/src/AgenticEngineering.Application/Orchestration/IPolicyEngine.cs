using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;
using AgenticEngineering.Domain.Models;

namespace AgenticEngineering.Application.Orchestration;

public interface IPolicyEngine
{
    PolicyDecision Evaluate(
        Workflow workflow,
        WorkflowNode node,
        AgentContext context);
}