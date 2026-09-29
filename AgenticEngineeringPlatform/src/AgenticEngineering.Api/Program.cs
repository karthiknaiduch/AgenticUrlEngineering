using AgenticEngineering.Application.Agents;
using AgenticEngineering.Application.Orchestration;
using AgenticEngineering.Infrastructure.Workflow;
using AgenticEngineering.Infrastructure.Orchestration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agents
builder.Services.AddScoped<IEngineeringAgent, RequirementAgent>();
builder.Services.AddScoped<IEngineeringAgent, ArchitectureAgent>();
builder.Services.AddScoped<IEngineeringAgent, TaskPlannerAgent>();
builder.Services.AddScoped<IEngineeringAgent, BackendImplementationAgent>();
builder.Services.AddScoped<IEngineeringAgent, FrontendImplementationAgent>();
builder.Services.AddScoped<IEngineeringAgent, DatabaseAgent>();
builder.Services.AddScoped<IEngineeringAgent, TestingAgent>();
builder.Services.AddScoped<IEngineeringAgent, SecurityAgent>();
builder.Services.AddScoped<IEngineeringAgent, ReleaseAgent>();
builder.Services.AddScoped<AgentContextAccessor>();
builder.Services.AddScoped<IDecisionRecorder, DecisionRecorder>();
builder.Services.AddScoped<IRollbackHandler, RollbackHandler>();

// Policy
builder.Services.AddScoped<IPolicyEngine, PolicyEngine>();

// Workflow planning
builder.Services.AddScoped<IWorkflowPlanner, WorkflowPlanner>();

// Workflow execution
builder.Services.AddScoped<WorkflowExecutor>();

// Orchestration
builder.Services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();
builder.Services.AddSingleton<IWorkflowStore, InMemoryWorkflowStore>();

var app = builder.Build();

// ---------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }