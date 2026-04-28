# Development Guide

This guide describes how to run ideaForge locally.

## Prerequisites

- .NET 10 SDK
- PostgreSQL available on `localhost:5432`
- Codex CLI available on `PATH` when testing Codex execution
- GitHub Copilot access when testing Copilot execution

## Build

```powershell
dotnet build D:\projects\IdeaForge\IdeaForge.slnx
```

## Database

The Web API reads `ConnectionStrings:IdeaForgeDb` from configuration.

Local development currently uses:

```text
src/IdeaForge.WebApi/appsettings.Development.json
```

The startup initializer will:

- create the configured database when it does not exist
- apply EF Core migrations
- create `persons`, `employees`, `agent_data`, `agent_executions`, `client_company`, `client_project`, and `project_repository`

Apply migrations manually:

```powershell
dotnet ef database update `
  --project D:\projects\IdeaForge\src\IdeaForge.Infrastructure `
  --startup-project D:\projects\IdeaForge\src\IdeaForge.Infrastructure `
  --context IdeaForgeDbContext
```

Add a new migration:

```powershell
dotnet ef migrations add <MigrationName> `
  --project D:\projects\IdeaForge\src\IdeaForge.Infrastructure `
  --startup-project D:\projects\IdeaForge\src\IdeaForge.Infrastructure `
  --context IdeaForgeDbContext `
  --output-dir Persistence\Migrations
```

For non-local environments, move credentials to user secrets, environment variables, or a secret manager.

## Run Web API

```powershell
dotnet run --project D:\projects\IdeaForge\src\IdeaForge.WebApi
```

Default local URL:

```text
http://localhost:5246
```

Swagger:

```text
http://localhost:5246/swagger
```

## Run React UI

Install dependencies:

```powershell
cd D:\projects\IdeaForge\src\IdeaForge.WebUi
npm install
```

Start the Vite development server:

```powershell
npm run dev
```

Default UI URL:

```text
http://localhost:5173
```

The development server proxies `/api` to `http://localhost:5246`, so start the Web API before using the UI. Override the proxy target with `VITE_IDEAFORGE_API_TARGET` when needed.

Build the UI:

```powershell
npm run build
```

## Docker Compose

Start the Web API container:

```powershell
docker compose up --build
```

The compose stack uses:

- `webapi`: ASP.NET Core Web API built from `Dockerfile`.
- `ConnectionStrings__IdeaForgeDb`: points the Web API container to the host PostgreSQL server through `host.docker.internal:5432`.

This means Docker Compose uses the same local PostgreSQL instance as normal development. The database name can still be isolated with `IDEA_FORGE_DB_NAME`.

Default Docker URL:

```text
http://localhost:5246/swagger
```

Copy `.env.example` to `.env` to override the API port, database name, host, user, or password. The `.env` file is intentionally ignored by git.

Stop the stack:

```powershell
docker compose down
```

## Common Verification

Build the solution:

```powershell
dotnet build D:\projects\IdeaForge\IdeaForge.slnx
```

Check git status:

```powershell
git status --short
```

Create a sample agent through Swagger or with `IdeaForge.WebApi.http`, then call:

```http
POST http://localhost:5246/api/agents/by-key/docs-writer/chat
```

## PowerShell Wrappers

Both wrapper scripts can report executions back to the Web API.

Official references:

- [GitHub Copilot CLI programmatic reference](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-programmatic-reference)
- [OpenAI Codex CLI reference](https://developers.openai.com/codex/cli/reference)
- [OpenAI model guide](https://developers.openai.com/api/docs/models)

Local model registries:

- `codex-models.json`: Codex model validation cache, including `gpt-5.5`.
- `copilot-models.json`: Copilot model validation cache.

Codex example:

```powershell
.\codex-agent.ps1 `
  -Prompt "Summarize this repository" `
  -Json `
  -Reasoning high `
  -Effort high `
  -Compute aggressive `
  -WorkingDirectory "D:\projects\IdeaForge" `
  -EmployeeKey "docs-writer" `
  -AgentApiBaseUrl "http://localhost:5246"
```

Codex tuning:

- `-Reasoning` maps to `codex exec --reasoning`.
- `-Effort` maps to `codex exec --effort`.
- `-Compute` maps to `codex exec --compute`.
- The Web API stores the same values as `agentSettings.codexReasoning`, `agentSettings.codexEffort`, and `agentSettings.codexCompute` on AI people.

Copilot example:

```powershell
.\copilot-agent.ps1 `
  -Prompt "Review this repository" `
  -OutputFormat json `
  -WorkingDirectory "D:\projects\IdeaForge" `
  -EmployeeKey "reviewer" `
  -AgentApiBaseUrl "http://localhost:5246"
```

POC permission defaults:

- Copilot wrapper defaults to `--allow-all-tools`, `--allow-all-urls`, and `--allow-all-paths`.
- Copilot SDK provider defaults to approving every permission request.
- Codex wrapper and C# provider default to `--dangerously-bypass-approvals-and-sandbox`; when that bypass is disabled, Codex falls back to `danger-full-access`.
- These defaults are intentionally broad for the company POC and should be tightened before production use.

Session behavior:

- Copilot JSON output includes `sessionId`; the wrapper can capture and report it automatically.
- Codex JSON output includes `thread_id`; the wrapper reports it as the session id.
- Manual `-SessionId` takes precedence when provided.

## Current Limitations

- Codex provider execution through the C# provider does not yet resume an existing Codex session.
- Copilot provider supports named sessions through the Copilot SDK.
- Authentication for external LLM tools is handled outside this repository.
