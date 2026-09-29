using AgenticEngineering.Application.Models;
using AgenticEngineering.Domain.Entities;

namespace AgenticEngineering.Application.Orchestration;

public class WorkflowPlanner : IWorkflowPlanner
{
    public Workflow CreateWorkflow(AgentContext context)
    {
        var workflowId = context.WorkflowId == Guid.Empty
            ? Guid.NewGuid()
            : context.WorkflowId;

        var requirementNode = CreateNode(
            workflowId,
            "RequirementAgent");

        var architectureNode = CreateNode(
            workflowId,
            "ArchitectureAgent",
            requirementNode.Id);

        var taskPlannerNode = CreateNode(
            workflowId,
            "TaskPlannerAgent",
            architectureNode.Id);

        var backendNode = CreateNode(
            workflowId,
            "BackendImplementationAgent",
            taskPlannerNode.Id);

        var frontendNode = CreateNode(
            workflowId,
            "FrontendImplementationAgent",
            taskPlannerNode.Id);

        var databaseNode = CreateNode(
            workflowId,
            "DatabaseAgent",
            taskPlannerNode.Id);

        var testingNode = CreateNode(
            workflowId,
            "TestingAgent",
            backendNode.Id,
            frontendNode.Id,
            databaseNode.Id);

        var securityNode = CreateNode(
            workflowId,
            "SecurityAgent",
            testingNode.Id);

        var releaseNode = CreateNode(
            workflowId,
            "ReleaseAgent",
            securityNode.Id);

        return new Workflow
        {
            Id = workflowId,
            Requirement = context.Requirement,
            Nodes =
            [
                requirementNode,
                architectureNode,
                taskPlannerNode,
                backendNode,
                frontendNode,
                databaseNode,
                testingNode,
                securityNode,
                releaseNode
            ]
        };
    }

    private static WorkflowNode CreateNode(
        Guid workflowId,
        string agentType,
        params Guid[] dependencies)
    {
        return new WorkflowNode
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            AgentType = agentType,
            DependsOn = dependencies.ToList(),
            MaxRetries = 3
        };
    }
}

