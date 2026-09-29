using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;

namespace AgenticEngineering.Application.Orchestration;

public interface IWorkflowPlanner
{
    Workflow CreateWorkflow(
        AgentContext context);
}