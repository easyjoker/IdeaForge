# Architecture

ideaForge uses a traditional three-layer application structure with reusable agent packages. The Web API is intentionally thin; application services own use cases, infrastructure owns persistence, and provider packages own LLM execution details.

## Solution Layout

```text
src/
  IdeaForge.Agents.Abstractions/
  IdeaForge.Agents.Copilot/
  IdeaForge.Agents.Codex/
  IdeaForge.Agents/
  IdeaForge.Application/
  IdeaForge.Infrastructure/
  IdeaForge.WebApi/
  IdeaForge.Agents.Console/
```

## Projects

`IdeaForge.Agents.Abstractions`

Defines shared contracts and domain models:

- `IAgentProvider`
- `AgentExecutionRequest`
- `AgentExecutionResult`
- `Person`
- `Employee`
- `AgentData`
- `AgentExecutionRecord`
- provider and status enums

`IdeaForge.Agents.Copilot`

Implements `IAgentProvider` with GitHub Copilot SDK. It supports named sessions and can resume an existing provider session when `AgentData.SessionId` is available.

`IdeaForge.Agents.Codex`

Implements `IAgentProvider` with Codex CLI execution. It maps optional Codex tuning values to `--reasoning`, `--effort`, and `--compute`. It currently starts Codex through the command line and does not yet resume an existing session through `AgentExecutionRequest.SessionId`.

`IdeaForge.Agents`

Provides provider registration and lookup through:

- `AgentProviderRegistry`
- `AddIdeaForgeAgents()`

`IdeaForge.Application`

Contains use cases and application-facing DTOs:

- creating and updating AI employee agents
- creating people separately from employee role assignments
- reporting completed executions
- sending chat prompts to a selected employee
- mapping domain models to API DTOs

`IdeaForge.Infrastructure`

Contains PostgreSQL persistence and database initialization:

- `IdeaForgeDbContext`
- EF Core migrations
- `PostgreSqlAgentProfileRepository`
- `PostgreSqlInitializer`
- `PostgreSqlOptions`

`IdeaForge.WebApi`

Exposes HTTP endpoints and Swagger documentation.

## Runtime Flow

The normal chat flow is:

1. Client calls `POST /api/agents/by-key/{key}/chat`.
2. Web API forwards the request to `IAgentConversationService`.
3. Application loads the employee profile from PostgreSQL.
4. Application selects the configured provider from registered `IAgentProvider` instances.
5. Provider executes the prompt.
6. Application updates `agent_data`.
7. Application inserts an `agent_executions` record.
8. Web API returns the provider output.

## Data Ownership

`persons`

Stores stable person identity for both human and AI personnel:

- kind (`Human` or `AI`)
- display name
- description
- metadata
- created and updated timestamps

`employees`

Stores the company employment/work-assignment record:

- person reference
- key
- role
- mission
- specialties
- metadata
- lifecycle status

`agent_data`

Stores current runtime state for AI people only:

- provider
- model
- session id
- system prompt
- Codex CLI tuning settings: reasoning, effort, and compute
- last used timestamp
- session updated timestamp

`agent_executions`

Stores execution history:

- prompt
- output summary
- success flag
- error message
- started and completed timestamps

`client_company`

Stores client company identity:

- key
- name
- description
- lifecycle status

`client_project`

Stores a client-owned system or application under a client company. The API contract keeps the `project` name, but the domain meaning is a system/product that the client company owns:

- client company reference
- key
- name
- description
- lifecycle status

`project_repository`

Stores one source repository that belongs to a client system/project. A single client project can have many repositories:

- client project/system reference
- key
- name
- remote repository URL
- local workspace path
- git strategy skill path
- description
- lifecycle status

## Provider Values

`AgentProviderKind` values:

- `0` = `Unknown`
- `1` = `Copilot`
- `2` = `Codex`

`AgentStatus` values:

- `0` = `Draft`
- `1` = `Active`
- `2` = `Disabled`
- `3` = `Archived`
