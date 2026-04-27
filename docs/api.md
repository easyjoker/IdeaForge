# API Guide

The Web API is available at the launch profile URL:

```text
http://localhost:5246
```

Swagger is available at:

```text
http://localhost:5246/swagger
```

## Create Agent

Endpoint:

```http
POST /api/agents
```

Example body:

```json
{
  "key": "docs-writer",
  "name": "Docs Writer",
  "role": "Documentation Agent",
  "description": "Writes and maintains project documentation.",
  "mission": "Keep ideaForge documentation accurate and useful.",
  "specialties": ["documentation", "api", "developer-experience"],
  "metadata": {
    "team": "platform"
  },
  "provider": 2,
  "model": "gpt-5.4",
  "systemPrompt": "Answer in concise Traditional Chinese."
}
```

Provider values:

- `1` = `Copilot`
- `2` = `Codex`

Status defaults to `Active` when creating an agent.

## List Agents

Endpoint:

```http
GET /api/agents
```

## Get Agent By Key

Endpoint:

```http
GET /api/agents/by-key/{key}
```

Example:

```http
GET /api/agents/by-key/docs-writer
```

## Update Agent

Endpoint:

```http
PUT /api/agents/{id}
```

Example body:

```json
{
  "name": "Docs Writer",
  "role": "Documentation Agent",
  "description": "Writes and maintains project documentation.",
  "mission": "Keep ideaForge documentation accurate and useful.",
  "status": 1,
  "specialties": ["documentation", "api", "developer-experience"],
  "metadata": {
    "team": "platform"
  },
  "provider": 2,
  "model": "gpt-5.4",
  "systemPrompt": "Answer in concise Traditional Chinese."
}
```

Status values:

- `0` = `Draft`
- `1` = `Active`
- `2` = `Disabled`
- `3` = `Archived`

## Chat With Agent

Endpoint:

```http
POST /api/agents/by-key/{key}/chat
```

Example body:

```json
{
  "prompt": "Summarize the current repository.",
  "workingDirectory": "D:\\projects\\IdeaForge",
  "additionalDirectories": []
}
```

The chat endpoint:

- loads the employee profile
- uses the configured provider and model
- passes the existing session id when the provider supports named sessions
- runs the prompt
- updates `agent_data`
- inserts an `agent_executions` record

## Report Execution

Endpoint:

```http
POST /api/agents/by-key/{key}/executions
```

This endpoint is useful when an external runner or PowerShell wrapper already executed the prompt and only needs to report the result.

Example body:

```json
{
  "provider": 2,
  "model": "gpt-5.4",
  "sessionId": "019db419-d4c1-7c30-a45b-2f483f270051",
  "prompt": "Summarize the current repository.",
  "outputSummary": "Repository summarized successfully.",
  "success": true,
  "errorMessage": null,
  "startedAtUtc": "2026-04-27T02:00:00Z",
  "completedAtUtc": "2026-04-27T02:01:00Z"
}
```

If `agent_data.session_id` is empty and the report contains `sessionId`, the service writes it back automatically.

## List Executions

Endpoint:

```http
GET /api/agents/by-key/{key}/executions
```

Example:

```http
GET /api/agents/by-key/docs-writer/executions
```

## Create Client Company

Endpoint:

```http
POST /api/client-companies
```

Example body:

```json
{
  "key": "contoso",
  "name": "Contoso Ltd.",
  "description": "Primary POC client company."
}
```

Status defaults to `Active` when creating a company.

## List Client Companies

Endpoint:

```http
GET /api/client-companies
```

## Get Client Company By Key

Endpoint:

```http
GET /api/client-companies/by-key/{key}
```

Example:

```http
GET /api/client-companies/by-key/contoso
```

## Update Client Company

Endpoint:

```http
PUT /api/client-companies/{id}
```

Example body:

```json
{
  "name": "Contoso Ltd.",
  "description": "Primary POC client company.",
  "status": 1
}
```

Client resource status values:

- `0` = `Draft`
- `1` = `Active`
- `2` = `Disabled`
- `3` = `Archived`

## Client Project Meaning

In ideaForge, a client project represents a system, product, or application owned by a client company. Keep using the `/client-projects` API routes, but treat the hierarchy as:

```text
client_company 1:N client_project(system) 1:N project_repository
```

## Create Client Project (System)

Endpoint:

```http
POST /api/client-companies/{companyId}/projects
```

Example body:

```json
{
  "key": "commerce-platform",
  "name": "Commerce Platform",
  "description": "Customer-facing commerce system."
}
```

## List Client Projects (Systems)

Endpoint:

```http
GET /api/client-companies/{companyId}/projects
```

## Update Client Project (System)

Endpoint:

```http
PUT /api/client-projects/{id}
```

Example body:

```json
{
  "name": "Commerce Platform",
  "description": "Customer-facing commerce system.",
  "status": 1
}
```

## Create Project Repository

Repositories are scoped to one client project/system. Use this for each source repository in the selected system, such as a microservice repository, frontend repository, or shared library repository.

Endpoint:

```http
POST /api/client-projects/{projectId}/repositories
```

Example body:

```json
{
  "key": "order-service",
  "name": "Order Service",
  "remoteRepositoryUrl": "https://github.com/contoso/order-service.git",
  "localPath": "D:\\projects\\clients\\contoso\\order-service",
  "gitStrategySkillPath": ".codex/skills/git-strategy/SKILL.md",
  "description": "Order workflow microservice."
}
```

## List Project Repositories

Endpoint:

```http
GET /api/client-projects/{projectId}/repositories
```

## Update Project Repository

Endpoint:

```http
PUT /api/project-repositories/{id}
```

Example body:

```json
{
  "name": "Order Service",
  "remoteRepositoryUrl": "https://github.com/contoso/order-service.git",
  "localPath": "D:\\projects\\clients\\contoso\\order-service",
  "gitStrategySkillPath": ".codex/skills/git-strategy/SKILL.md",
  "description": "Order workflow microservice.",
  "status": 1
}
```
