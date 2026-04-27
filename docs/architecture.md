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
db/
  postgresql/
```

## Projects

`IdeaForge.Agents.Abstractions`

Defines shared contracts and domain models:

- `IAgentProvider`
- `AgentExecutionRequest`
- `AgentExecutionResult`
- `Employee`
- `AgentData`
- `AgentExecutionRecord`
- provider and status enums

`IdeaForge.Agents.Copilot`

Implements `IAgentProvider` with GitHub Copilot SDK. It supports named sessions and can resume an existing provider session when `AgentData.SessionId` is available.

`IdeaForge.Agents.Codex`

Implements `IAgentProvider` with Codex CLI execution. It currently starts Codex through the command line and does not yet resume an existing session through `AgentExecutionRequest.SessionId`.

`IdeaForge.Agents`

Provides provider registration and lookup through:

- `AgentProviderRegistry`
- `AddIdeaForgeAgents()`

`IdeaForge.Application`

Contains use cases and application-facing DTOs:

- creating and updating employee-style agents
- reporting completed executions
- sending chat prompts to a selected employee
- mapping domain models to API DTOs

`IdeaForge.Infrastructure`

Contains PostgreSQL persistence and database initialization:

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

`employees`

Stores stable employee identity:

- key
- name
- role
- mission
- specialties
- metadata
- lifecycle status

`agent_data`

Stores current runtime state:

- provider
- model
- session id
- system prompt
- last used timestamp
- session updated timestamp

`agent_executions`

Stores execution history:

- prompt
- output summary
- success flag
- error message
- started and completed timestamps

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
