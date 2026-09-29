# Agentic Engineering Platform

An agentic software engineering orchestration platform prototype that demonstrates how AI-assisted engineering workflows can be executed through a **controlled, stateful, policy-driven orchestration framework**.

The platform separates **agent reasoning** from **workflow orchestration**.

> **The LLM is not the orchestrator.**

Agents perform bounded engineering tasks such as requirement analysis, architecture planning, task decomposition, implementation planning, testing, and security analysis.

The deterministic workflow orchestrator controls:

* Workflow state
* Dependency execution
* Sequential and parallel execution
* Human approvals
* Retry policies
* Failure classification
* Policy enforcement
* Safe-stop behavior
* Rollback
* Context propagation
* Decision lineage
* Artifact tracking

This project is currently a prototype focused on validating the orchestration architecture and engineering workflow behavior.

---

## 1. Project Goals

The platform was designed to demonstrate an engineering system capable of handling:

* Greenfield engineering requirements
* Stateful multi-step workflows
* Dependency-aware task execution
* Parallel engineering activities
* Human-in-the-loop approvals
* Controlled retries
* Failure classification
* Security and policy guardrails
* Rollback and compensation
* Decision traceability
* Engineering artifacts
* Workflow inspection and status tracking

The architecture is intentionally designed so that deterministic orchestration remains in control even when individual agents are later backed by LLMs.

---

# 2. Core Architecture

```text
                         User Requirement
                                |
                                v
                    +------------------------+
                    | Engineering API        |
                    +-----------+------------+
                                |
                                v
                    +------------------------+
                    | Workflow Orchestrator  |
                    +-----------+------------+
                                |
                                v
                    +------------------------+
                    | Workflow Planner       |
                    | Dependency DAG         |
                    +-----------+------------+
                                |
          +---------------------+---------------------+
          |                     |                     |
          v                     v                     v
 +----------------+    +----------------+    +----------------+
 | Requirement    |    | Architecture   |    | Task Planner   |
 | Agent          |    | Agent          |    | Agent          |
 +----------------+    +----------------+    +----------------+
                                |
                                v
                       +----------------+
                       | Implementation |
                       | Planning       |
                       +-------+--------+
                               |
              +----------------+----------------+
              |                |                |
              v                v                v
       +-------------+  +-------------+  +-------------+
       | Backend     |  | Frontend    |  | Database    |
       | Agent       |  | Agent       |  | Agent       |
       +-------------+  +-------------+  +-------------+
              \                |                /
               \_______________|_______________/
                               |
                               v
                       +---------------+
                       | Testing Agent |
                       +-------+-------+
                               |
                               v
                       +---------------+
                       | Security Agent|
                       +-------+-------+
                               |
                         Human Approval
                               |
                               v
                       +---------------+
                       | Release Agent |
                       +---------------+
```

---

# 3. Core Design Principle

The most important architectural principle is:

```text
LLM / Agent
    |
    | bounded reasoning
    v
Agent Result
    |
    v
Deterministic Orchestrator
    |
    +--> State
    +--> Dependencies
    +--> Policies
    +--> Retries
    +--> Approvals
    +--> Rollback
    +--> Safe Stop
    +--> Audit / Decisions
```

The agents do not control the overall workflow.

The orchestrator determines:

* What can execute
* When it can execute
* Whether approval is required
* Whether a failure should be retried
* Whether execution should stop
* Whether rollback should occur
* Whether a policy violation requires a safe stop

This separation prevents an AI agent from independently making uncontrolled execution decisions.

---

# 4. Technology Stack

## Backend

* .NET 8
* ASP.NET Core Web API
* C#
* Dependency Injection
* Async/await
* REST APIs

## Architecture

* Clean architecture-inspired separation
* Domain-driven entities
* Application services
* Infrastructure implementations
* Dependency Injection
* Dependency graph / DAG workflow execution

## Current Prototype

* In-memory workflow persistence
* Deterministic agent implementations
* Swagger/OpenAPI
* xUnit-compatible project structure

---

# 5. Solution Structure

```text
AgenticEngineeringPlatform
│
├── src
│   │
│   ├── AgenticEngineering.Api
│   │   ├── Controllers
│   │   ├── Models
│   │   └── Program.cs
│   │
│   ├── AgenticEngineering.Application
│   │   ├── Agents
│   │   ├── Models
│   │   └── Orchestration
│   │
│   ├── AgenticEngineering.Domain
│   │   ├── Entities
│   │   ├── Enums
│   │   └── Models
│   │
│   └── AgenticEngineering.Infrastructure
│       ├── Orchestration
│       ├── Persistence
│       └── Policy
│
└── tests
    └── AgenticEngineering.UnitTests
```

---

# 6. Workflow Model

A workflow contains:

```text
Workflow
 ├── WorkflowId
 ├── Requirement
 ├── Status
 ├── Version
 ├── CreatedAtUtc
 ├── UpdatedAtUtc
 └── Nodes
```

Each node represents an engineering agent.

```text
WorkflowNode
 ├── NodeId
 ├── WorkflowId
 ├── AgentType
 ├── DependsOn
 ├── Status
 ├── RetryCount
 ├── MaxRetries
 ├── RequiresApproval
 ├── ApprovalId
 └── ApprovalStatus
```

---

# 7. Workflow States

## Workflow Status

```text
Created
Planning
Running
WaitingForApproval
Blocked
Retrying
Failed
RollingBack
Completed
Cancelled
SafeStopped
```

## Node Status

```text
Pending
Running
WaitingForApproval
Completed
Failed
Retrying
Skipped
RolledBack
SafeStopped
```

These states allow the workflow to remain observable and resumable.

---

# 8. Dependency DAG

The workflow is represented as a dependency graph.

```text
Requirement
     |
     v
Architecture
     |
     v
Task Planner
     |
     +----------+----------+
     |          |          |
     v          v          v
 Backend     Frontend   Database
     |          |          |
     +----------+----------+
                |
                v
             Testing
                |
                v
             Security
                |
                v
             Release
```

The executor determines which nodes are ready by checking whether all dependencies have completed.

This provides:

* Dependency awareness
* Sequential execution
* Parallel execution
* Synchronization
* Blocking when dependencies are incomplete

---

# 9. Parallel Execution

Backend, Frontend, and Database work are independent after task planning.

They are therefore executed in parallel:

```text
TaskPlanner
     |
     +----------+----------+
     |          |          |
     v          v          v
 Backend    Frontend    Database
     |          |          |
     +----------+----------+
                |
                v
             Testing
```

Testing cannot begin until all three dependencies complete.

The implementation uses `Task.WhenAll()` for the ready nodes.

---

# 10. Agent Architecture

All engineering agents implement:

```text
IEngineeringAgent
```

with:

```text
AgentType
ExecuteAsync()
```

The standardized result is:

```text
AgentResult
 ├── Status
 ├── Message
 ├── FailureType
 ├── Artifacts
 ├── Errors
 └── Data
```

This gives the orchestrator a consistent contract regardless of which agent is executing.

---

# 11. Implemented Agents

## RequirementAgent

Responsible for:

* Requirement validation
* Requirement normalization
* Initial requirement interpretation

---

## ArchitectureAgent

Responsible for:

* Creating an initial architecture proposal
* Recording architecture decisions
* Capturing alternatives
* Capturing evidence supporting the decision

Example recorded decision:

```text
Decision:
Select a service-oriented REST architecture.

Alternatives:
- Monolithic application
- Service-oriented architecture
- Event-driven architecture
```

---

## TaskPlannerAgent

Breaks the architecture into engineering tasks.

Example:

```text
Backend
- Create API
- Implement business services
- Implement REST endpoints
- Implement persistence

Frontend
- Create Angular application
- Implement API integration
- Implement UI components

Database
- Create schema
- Create migrations
- Add indexes

Testing
- Unit tests
- Integration tests
- API validation

Security
- Authentication
- Authorization
- Input validation
- Secret management
```

---

## BackendImplementationAgent

Represents bounded backend implementation work.

---

## FrontendImplementationAgent

Represents bounded frontend implementation work.

---

## DatabaseAgent

Represents database engineering work.

This agent is protected by a human approval gate.

---

## TestingAgent

Represents:

* Unit testing
* Integration testing
* API validation

Testing depends on:

```text
Backend
Frontend
Database
```

---

## SecurityAgent

Responsible for security validation.

Security failures are deliberately classified separately from generic implementation failures.

Security failures are **not automatically retried**.

---

## ReleaseAgent

Represents release/deployment activities.

Production release requires human approval.

---

# 12. Agent Context

Agents share controlled workflow context through:

```text
AgentContext
```

The context contains:

```text
WorkflowId
Requirement
Architecture
Artifacts
PreviousFailures
Constraints
Decisions
Approvals
ApprovalRecords
Data
```

This allows information produced by one agent to become available to downstream agents.

---

# 13. Retry Policy

Failures are classified.

Current retry behavior:

| Failure Type          | Retry |
| --------------------- | ----- |
| Transient             | Yes   |
| Unknown               | Yes   |
| ImplementationFailure | Yes   |
| Validation            | No    |
| PolicyViolation       | No    |
| SecurityFailure       | No    |
| DependencyFailure     | No    |

Retries use bounded exponential backoff:

```text
Retry 1 → 1 second
Retry 2 → 2 seconds
Retry 3 → 4 seconds
```

The node has a maximum retry count.

This prevents infinite retry loops.

---

# 14. Security Failure Handling

Security failures are intentionally not retried.

Example:

```text
SecurityAgent
      |
      v
SecurityFailure
      |
      v
ShouldRetry() = false
      |
      v
Workflow Failure
      |
      v
Rollback
```

This is important because repeatedly retrying a security validation failure does not necessarily resolve the underlying security problem.

---

# 15. Human-in-the-Loop Approval

Certain operations require explicit approval.

Currently:

```text
DatabaseAgent → Approval Required
ReleaseAgent  → Approval Required
```

The workflow does not block an HTTP request while waiting.

Instead:

```text
Workflow
   |
   v
WaitingForApproval
   |
   | Human approval
   v
ResumeAsync()
   |
   v
Continue workflow
```

This allows the workflow to persist its state and resume later.

---

# 16. Policy Engine

The policy engine evaluates every node before execution.

Example protected operation:

```text
Delete production database
```

The policy engine returns:

```text
Allowed = false
Reason = Production database deletion is prohibited
Violation = Destructive production database operation detected
```

The workflow then enters:

```text
SafeStopped
```

No downstream agents are executed.

---

# 17. Safe Stop

Safe-stop is different from normal failure.

```text
Policy violation
      |
      v
SafeStopped
      |
      X
No execution
```

The purpose is to prevent a dangerous operation from proceeding.

Example:

```text
"Delete production database records"
```

results in:

```text
Workflow = SafeStopped
Node = SafeStopped
```

rather than executing the destructive operation.

---

# 18. Rollback

If an unrecoverable failure occurs after earlier nodes have completed:

```text
Failure
   |
   v
RollbackHandler
   |
   +--> Completed nodes become RolledBack
   |
   +--> Failed node remains Failed
   |
   +--> Downstream nodes remain Pending
```

Example:

```text
Requirement       → RolledBack
Architecture      → RolledBack
TaskPlanner       → RolledBack
Backend            → RolledBack
Frontend           → RolledBack
Database           → RolledBack
Testing            → RolledBack
Security           → Failed
Release            → Pending
```

The current prototype models rollback through workflow state transitions.

Future versions can add actual compensating actions for each agent.

---

# 19. Decision Lineage

Engineering decisions are represented by:

```text
Decision
 ├── DecisionId
 ├── WorkflowId
 ├── DecisionText
 ├── Reason
 ├── Evidence
 ├── Alternatives
 ├── SelectedOption
 ├── Actor
 └── TimestampUtc
```

Example:

```text
Decision:
Select a service-oriented REST architecture.

Reason:
The requirement requires independently deployable backend
capabilities with synchronous API access.

Evidence:
- Requirement specifies API-based transaction processing.
- Backend and frontend need a clear service boundary.

Alternatives:
- Monolithic application
- Service-oriented architecture
- Event-driven architecture

Selected:
Service-oriented REST architecture

Actor:
ArchitectureAgent
```

This provides traceability for why an architectural choice was made.

---

# 20. Workflow Persistence

The current prototype uses:

```text
InMemoryWorkflowStore
```

This provides workflow persistence during application execution and supports:

* Save workflow
* Retrieve workflow
* Resume workflow
* Inspect workflow state

For production, this would be replaced with durable persistence such as PostgreSQL or SQL Server.

A production implementation would also add optimistic concurrency/version checks.

---

# 21. API Endpoints

## Create Workflow

```http
POST /api/v1/workflows
```

Example:

```json
{
  "requirement": "Build a customer transaction management API."
}
```

---

## Get Workflow

```http
GET /api/v1/workflows/{workflowId}
```

Returns:

* Workflow status
* Node statuses
* Dependencies
* Retry counts
* Approval information
* Artifacts
* Previous failures
Decisions
Approve Workflow
POST /api/v1/workflows/{workflowId}/approve

Example:

{
  "approvedBy": "engineering-manager",
  "reason": "Approved database changes."
}
**22. Validation Scenarios**

The prototype has been exercised against three important scenarios.

Scenario 1 — Successful Workflow
Requirement
    ↓
Architecture
    ↓
Task Planning
    ↓
Backend + Frontend + Database
    ↓
Testing
    ↓
Security
    ↓
Database Approval
    ↓
Release Approval
    ↓
Completed

Expected final state:

Workflow = Completed

All required nodes complete after their respective approval gates.

Scenario 2 — Security Failure

Requirement:

Build a customer transaction management API
and force security failure.

Execution:

Requirement
Architecture
Task Planner
Backend
Frontend
Database
Testing
     ↓
Security Failure
     ↓
No retry
     ↓
Rollback
     ↓
Workflow Failed

Security failures are classified as:

FailureType.SecurityFailure

and are not automatically retried.

Scenario 3 — Policy Safe Stop

Requirement:

Delete production database records
and rebuild the production database.

Policy evaluation detects a prohibited destructive operation.

Result:

Workflow = SafeStopped

The workflow does not continue to database execution or downstream agents.

**23. Greenfield and Brownfield**
Greenfield

The current prototype primarily demonstrates a greenfield workflow:

Requirement
    ↓
Architecture
    ↓
Task decomposition
    ↓
Implementation
    ↓
Testing
    ↓
Release
Brownfield

The platform architecture is designed to support brownfield workflows where an existing repository is analyzed before implementation.

A future brownfield flow would be:

Existing Repository
        ↓
Repository Analysis
        ↓
Existing Architecture
        ↓
Impact Analysis
        ↓
Affected Components
        ↓
Implementation Plan
        ↓
Change
        ↓
Regression Testing

The existing URL Shortener project can serve as the brownfield repository for future implementation.

**24. Current Prototype Limitations**

This project intentionally focuses on orchestration rather than production infrastructure.

Current limitations include:

Workflow persistence is in-memory
Agents are deterministic prototype implementations
No production LLM provider is currently required
Rollback currently models state compensation rather than executing real compensating operations
Approval records can be further refined to create a pending record when the gate is reached
Approval can be made more granular by targeting a specific node
Production-grade telemetry and metrics can be added
Brownfield repository analysis can be added
Actual code-generation agents can be connected later
Release execution is currently represented by a bounded agent

These are extension points rather than prerequisites for the orchestration prototype.

**25. Why the Architecture Is Agentic**

The system is not simply a sequence of API calls.

It contains:

Multiple specialized engineering agents
Shared engineering context
Dependency-aware execution
Parallel execution
Stateful workflow management
Dynamic failure handling
Human approval gates
Policy enforcement
Decision lineage
Rollback behavior
Controlled autonomy

The key distinction is:

AI Agent = Reasoning capability

Orchestrator = Control plane

This separation allows AI capabilities to evolve without giving the AI uncontrolled authority over workflow execution.

**26. Future Extensions**

Potential future capabilities include:

LLM Integration

Connect agents to:

Azure OpenAI
OpenAI
Claude
Other enterprise-approved LLM providers

The existing IEngineeringAgent contract can remain unchanged.

Brownfield Repository Analysis

Add:

RepositoryAnalysisAgent

capable of analyzing:

Projects
Controllers
Services
Entities
APIs
Dependencies
Tests
Configuration
Durable Workflow State

Replace:

InMemoryWorkflowStore

with:

PostgreSQL / SQL Server

and add:

Optimistic concurrency
Workflow versioning
Durable approvals
Recovery after process restart
Observability

Add:

Structured logging
Correlation IDs
Metrics
Distributed tracing
Application Insights / OpenTelemetry
Workflow execution dashboards
Real Compensation

Replace simple state rollback with agent-specific compensation:

BackendAgent
    ↳ RollbackBackendChange()

DatabaseAgent
    ↳ RollbackMigration()

FrontendAgent
    ↳ RevertFrontendChange()
**27. Key Interview Discussion**

The following architectural principles are central to the project.

Why not let the LLM orchestrate?

Because the LLM should not independently control:

Production deployment
Database changes
Retry behavior
Security policy
Approval requirements
Rollback
Workflow state

The deterministic orchestrator provides those controls.

How are failures handled?

Failures are classified.

Transient
   → Retry

Implementation
   → Bounded Retry

Validation
   → Fail

Security
   → Fail

Policy Violation
   → Safe Stop

Unrecoverable Failure
   → Rollback
Why human approval?

Certain operations have a higher risk profile.

Examples:

Database changes
Production release

The workflow persists its state and waits for a human decision rather than keeping a request open.

How does parallel execution work?

Independent nodes are identified from the dependency graph and executed concurrently.

For example:

Backend
Frontend
Database

can execute concurrently because they all depend only on Task Planning.

Testing waits for all three.

How is traceability achieved?

The workflow records:

Decisions
Evidence
Alternatives
Selected options
Actor
Timestamp
Approvals
Failures
Retry counts
Artifacts
Node states

This creates a decision lineage for the engineering workflow.

**28. Running the Project**

From the repository root:

dotnet build

Run the API:

dotnet run --project src/AgenticEngineering.Api

Swagger is available when running in the Development environment.

Use the Swagger UI to:

Create a workflow
Inspect workflow state
Approve gated operations
Resume execution
Inspect final results
Test failure and safe-stop scenarios
**29. Summary**

This prototype demonstrates a controlled agentic software engineering system where:

AI provides reasoning
        +
Deterministic orchestration provides control
        +
Policies provide safety
        +
Human approvals provide governance
        +
Workflow state provides resilience
        +
Decision lineage provides traceability

The architecture is intentionally designed so that more capable AI agents can be introduced later without transferring control of execution policy away from the orchestration layer.
