# ideaForge

ideaForge is a software company and engineering platform built around employee-style AI agents. The current repository contains the first service foundation for defining agents, assigning LLM providers, executing prompts, and persisting runtime history in PostgreSQL.

The project currently supports two provider paths:

- GitHub Copilot through the Copilot SDK and `copilot-agent.ps1`
- OpenAI Codex through the Codex CLI and `codex-agent.ps1`

## What This Repository Contains

- Reusable C# agent packages under `src/IdeaForge.Agents.*`
- ASP.NET Core 10 Web API under `src/IdeaForge.WebApi`
- Application layer use cases under `src/IdeaForge.Application`
- PostgreSQL persistence under `src/IdeaForge.Infrastructure`
- EF Core migrations under `src/IdeaForge.Infrastructure/Persistence/Migrations`
- PowerShell wrappers for Copilot and Codex execution

## Documentation

- [Architecture](docs/architecture.md)
- [API Guide](docs/api.md)
- [Development Guide](docs/development.md)

## Provider References

- [GitHub Copilot CLI programmatic reference](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-programmatic-reference)
- [OpenAI Codex CLI reference](https://developers.openai.com/codex/cli/reference)

## Requirements

- .NET 10 SDK
- PostgreSQL on `localhost:5432`
- Codex CLI available on `PATH` when using the Codex provider
- GitHub Copilot authentication when using the Copilot provider

## Quick Start

Build the solution:

```powershell
dotnet build D:\projects\IdeaForge\IdeaForge.slnx
```

Run the Web API:

```powershell
dotnet run --project D:\projects\IdeaForge\src\IdeaForge.WebApi
```

Open Swagger:

```text
http://localhost:5246/swagger
```

Run the React UI:

```powershell
cd D:\projects\IdeaForge\src\IdeaForge.WebUi
npm install
npm run dev
```

The UI is available at:

```text
http://localhost:5173
```

Run with Docker Compose:

```powershell
docker compose up --build
```

Docker Compose starts the Web API and connects it to the host PostgreSQL server on `localhost:5432` through `host.docker.internal`. The API is available at:

```text
http://localhost:5246/swagger
```

## Core Model

ideaForge separates company personnel and agents into three concerns:

- `person`: stable person identity, such as human/AI kind, display name, description, and metadata
- `employee`: company work assignment, such as employee key, role, mission, specialties, and lifecycle status
- `agent_data`: AI-only runtime configuration, such as provider, model, session id, and system prompt
- `agent_executions`: historical records for completed prompt executions

This lets human and AI employees share the same personnel model while keeping LLM runtime state separate and optional.

Client delivery settings use this hierarchy:

- `client_company`: the external customer company
- `client_project`: one client-owned system, product, or application under that company
- `project_repository`: one source repository under that client project/system; each system can have many repositories

## Main API Flow

1. Create a person with `POST /api/people`.
2. Set that person's employee role with `POST /api/employees`.
3. Include `agentSettings` only when the selected person is AI.
4. Send a prompt to that AI employee with `POST /api/agents/by-key/{key}/chat`.
5. Read execution history with `GET /api/agents/by-key/{key}/executions`.

The chat endpoint executes the configured provider and persists the execution result automatically.

## PowerShell Wrappers

Run Codex and report execution back to the API:

```powershell
.\codex-agent.ps1 `
  -Prompt "Summarize this repository" `
  -Json `
  -EmployeeKey "docs-writer" `
  -AgentApiBaseUrl "http://localhost:5246"
```

Run Copilot and report execution back to the API:

```powershell
.\copilot-agent.ps1 `
  -Prompt "Review this repository" `
  -OutputFormat json `
  -EmployeeKey "reviewer" `
  -AgentApiBaseUrl "http://localhost:5246"
```

Both wrappers default to `gpt-5.4`.

Model registries:

- `codex-models.json` caches Codex model IDs, including `gpt-5.5`.
- `copilot-models.json` caches GitHub Copilot model IDs.

## Notes

- Swagger includes XML comments and enum descriptions for API discoverability.
- Local PostgreSQL initialization runs when the Web API starts.
- Development credentials should stay local. Use user secrets or environment variables before deploying beyond local development.
