---
name: task-start-commit-policy
description: Use at the beginning of every task in this repository to enforce task-start git checks, validation, and the rule that any repository modifications must be committed with a logical commit message.
---

# Task Start Commit Policy

## Rules

1. If any repository files are modified, run appropriate validation and create a `git commit` before finishing the task.
2. At task start, read this skill and run `git status --short`.
3. Treat existing uncommitted changes as user-owned unless they are clearly made by the current task.
4. Do not revert, overwrite, or remove user-owned changes unless the user explicitly asks.
5. Before committing, run `git status --short` and inspect the staged changes.
6. Stage only files related to the current task.
7. Do not commit build outputs, dependency caches, logs, local secrets, or generated binaries.
8. Push only when the user asks to publish or push the work.

## Commit Message Policy

Write commit messages that explain the logical purpose of the change, not a file inventory.

Good examples:

```text
Add agent chat execution flow
Document local development workflow
Validate Copilot model selection
Persist agent execution history
```

Avoid messages that only say which files changed:

```text
Update README and API docs
Modify AgentProfileService.cs
Change WebApi files
```

Use an imperative sentence. Prefer the behavior, capability, policy, or intent introduced by the change.

## Workflow

At the beginning of every task:

```powershell
git status --short
```

Before editing:

- Identify the expected change area.
- Notice dirty files before touching anything.
- Work around unrelated user changes rather than reverting them.

After editing:

- Run the smallest meaningful validation for the change.
- Run `git status --short`.
- Stage explicit paths.
- Commit with a logical message.

For this .NET repository, the default validation is:

```powershell
dotnet build D:\projects\IdeaForge\IdeaForge.slnx
```

If validation cannot be run, state why and still commit completed repository changes unless the user has explicitly asked not to commit.

## Commit Hygiene

Prefer explicit staging:

```powershell
git add path\to\file1 path\to\file2
git commit -m "Describe the logical change"
```

Avoid broad staging when unrelated files are dirty.

Before committing, confirm common local outputs are not staged:

- `bin/`
- `obj/`
- `*.log`
- `*.tmp`
- local secret files
