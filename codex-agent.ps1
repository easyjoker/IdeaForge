[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Prompt,

    [Parameter()]
    [string]$Model = "gpt-5.4",

    [Parameter()]
    [string]$ModelRegistryPath,

    [Parameter()]
    [string]$Profile,

    [Parameter()]
    [string]$WorkingDirectory,

    [Parameter()]
    [string[]]$AddDir,

    [Parameter()]
    [string[]]$Config,

    [Parameter()]
    [string[]]$ImagePath,

    [Parameter()]
    [string]$OutputPath,

    [Parameter()]
    [string]$AgentApiBaseUrl,

    [Parameter()]
    [Alias("AgentId")]
    [string]$EmployeeId,

    [Parameter()]
    [Alias("AgentKey")]
    [string]$EmployeeKey,

    [Parameter()]
    [string]$SessionId,

    [Parameter()]
    [string]$ExecutionSummary,

    [Parameter()]
    [string]$SchemaPath,

    [Parameter()]
    [ValidateSet("low", "medium", "high")]
    [string]$Reasoning,

    [Parameter()]
    [ValidateSet("low", "medium", "high")]
    [string]$Effort,

    [Parameter()]
    [ValidateSet("low", "medium", "high")]
    [string]$Compute,

    [Parameter()]
    [ValidateSet("always", "never", "auto")]
    [string]$Color = "auto",

    [Parameter()]
    [ValidateSet("untrusted", "on-request", "never")]
    [string]$AskForApproval = "never",

    [Parameter()]
    [ValidateSet("read-only", "workspace-write", "danger-full-access")]
    [string]$Sandbox = "danger-full-access",

    [Parameter()]
    [switch]$Json,

    [Parameter()]
    [switch]$Ephemeral,

    [Parameter()]
    [switch]$SkipGitRepoCheck,

    [Parameter()]
    [switch]$Oss,

    [Parameter()]
    [bool]$DangerouslyBypassApprovalsAndSandbox = $true
)

begin {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"

    function Get-ExecutionSummary {
        param(
            [string]$PreferredSummary,
            [object[]]$CommandOutput,
            [string]$FallbackError
        )

        if (-not [string]::IsNullOrWhiteSpace($PreferredSummary)) {
            return $PreferredSummary.Trim()
        }

        $text = ""
        if ($CommandOutput) {
            $text = (($CommandOutput | ForEach-Object { "$_" }) -join [Environment]::NewLine).Trim()
        }

        if (-not [string]::IsNullOrWhiteSpace($text)) {
            if ($text.Length -gt 1000) {
                return $text.Substring(0, 1000)
            }

            return $text
        }

        if (-not [string]::IsNullOrWhiteSpace($FallbackError)) {
            return $FallbackError.Trim()
        }

        return $null
    }

    function Get-CodexSessionId {
        param(
            [object[]]$CommandOutput
        )

        foreach ($entry in ($CommandOutput | Where-Object { $_ })) {
            $line = "$_".Trim()
            if (-not $line.StartsWith("{")) {
                continue
            }

            try {
                $json = $line | ConvertFrom-Json -Depth 10
                if ($json.type -eq "thread.started" -and -not [string]::IsNullOrWhiteSpace($json.thread_id)) {
                    return $json.thread_id
                }
            }
            catch {
            }
        }

        return $null
    }

    function Send-AgentExecutionReport {
        param(
            [string]$BaseUrl,
            [string]$TargetEmployeeId,
            [string]$TargetEmployeeKey,
            [hashtable]$Payload
        )

        if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
            return
        }

        if ([string]::IsNullOrWhiteSpace($TargetEmployeeId) -and [string]::IsNullOrWhiteSpace($TargetEmployeeKey)) {
            return
        }

        $trimmedBaseUrl = $BaseUrl.TrimEnd('/')
        if (-not [string]::IsNullOrWhiteSpace($TargetEmployeeId)) {
            $uri = "$trimmedBaseUrl/api/agents/$TargetEmployeeId/executions"
        }
        else {
            $escapedKey = [System.Uri]::EscapeDataString($TargetEmployeeKey)
            $uri = "$trimmedBaseUrl/api/agents/by-key/$escapedKey/executions"
        }

        try {
            $body = $Payload | ConvertTo-Json -Depth 5
            Invoke-RestMethod -Method Post -Uri $uri -ContentType "application/json" -Body $body | Out-Null
        }
        catch {
            Write-Warning "Failed to report agent execution to '$uri'. $($_.Exception.Message)"
        }
    }
}

process {
    $codexCommand = Get-Command codex -ErrorAction SilentlyContinue
    if (-not $codexCommand) {
        throw "The 'codex' command was not found in PATH."
    }

    if ($Model) {
        $resolvedModelRegistryPath = $ModelRegistryPath
        if (-not $resolvedModelRegistryPath) {
            $resolvedModelRegistryPath = Join-Path -Path $PSScriptRoot -ChildPath "codex-models.json"
        }

        $resolvedModelRegistryPath = [System.IO.Path]::GetFullPath($resolvedModelRegistryPath)

        if (-not (Test-Path -LiteralPath $resolvedModelRegistryPath)) {
            throw "Codex model registry not found: $resolvedModelRegistryPath"
        }

        $modelRegistry = Get-Content -LiteralPath $resolvedModelRegistryPath -Raw | ConvertFrom-Json
        $allowedModels = @($modelRegistry.models | ForEach-Object { $_.id })

        if ($allowedModels -notcontains $Model) {
            throw "Model '$Model' was not found in the Codex model registry: $resolvedModelRegistryPath"
        }
    }

    $args = @()

    if ($AddDir) {
        foreach ($dir in ($AddDir | Where-Object { $_ })) {
            $resolvedDir = [System.IO.Path]::GetFullPath($dir)
            $args += "--add-dir"
            $args += $resolvedDir
        }
    }

    if ($AskForApproval) {
        $args += "--ask-for-approval"
        $args += $AskForApproval
    }

    if ($DangerouslyBypassApprovalsAndSandbox) {
        $args += "--dangerously-bypass-approvals-and-sandbox"
    }

    $args += "exec"

    if ($WorkingDirectory) {
        $resolvedWorkingDirectory = [System.IO.Path]::GetFullPath($WorkingDirectory)
        $args += "--cd"
        $args += $resolvedWorkingDirectory
    }

    if ($Color) {
        $args += "--color"
        $args += $Color
    }

    if ($Ephemeral) {
        $args += "--ephemeral"
    }

    if ($Json) {
        $args += "--json"
    }

    if ($Model) {
        $args += "--model"
        $args += $Model
    }

    if ($Oss) {
        $args += "--oss"
    }

    if ($OutputPath) {
        $args += "--output-last-message"
        $args += [System.IO.Path]::GetFullPath($OutputPath)
    }

    if ($SchemaPath) {
        $args += "--output-schema"
        $args += [System.IO.Path]::GetFullPath($SchemaPath)
    }

    if ($Profile) {
        $args += "--profile"
        $args += $Profile
    }

    if ($Reasoning) {
        $args += "--reasoning"
        $args += $Reasoning
    }

    if ($Effort) {
        $args += "--effort"
        $args += $Effort
    }

    if ($Compute) {
        $args += "--compute"
        $args += $Compute
    }

    if ($Sandbox -and -not $DangerouslyBypassApprovalsAndSandbox) {
        $args += "--sandbox"
        $args += $Sandbox
    }

    if ($SkipGitRepoCheck) {
        $args += "--skip-git-repo-check"
    }

    if ($Config) {
        foreach ($entry in ($Config | Where-Object { $_ })) {
            $args += "--config"
            $args += $entry
        }
    }

    if ($ImagePath) {
        foreach ($path in ($ImagePath | Where-Object { $_ })) {
            $resolvedImagePath = [System.IO.Path]::GetFullPath($path)
            $args += "--image"
            $args += $resolvedImagePath
        }
    }

    $args += $Prompt

    $startedAtUtc = [DateTimeOffset]::UtcNow
    $commandOutput = @()
    $errorText = $null

    try {
        & $codexCommand.Source @args 2>&1 | Tee-Object -Variable commandOutput
        $exitCode = $LASTEXITCODE
    }
    catch {
        $exitCode = 1
        $errorText = $_.Exception.Message
        throw
    }
    finally {
        $completedAtUtc = [DateTimeOffset]::UtcNow

        if ($AgentApiBaseUrl -and ($EmployeeId -or $EmployeeKey)) {
            $resolvedSessionId = $SessionId
            if ([string]::IsNullOrWhiteSpace($resolvedSessionId) -and $Json) {
                $resolvedSessionId = Get-CodexSessionId -CommandOutput $commandOutput
            }

            $summary = Get-ExecutionSummary -PreferredSummary $ExecutionSummary -CommandOutput $commandOutput -FallbackError $errorText
            $payload = @{
                provider = 2
                model = $Model
                sessionId = if ([string]::IsNullOrWhiteSpace($resolvedSessionId)) { $null } else { $resolvedSessionId.Trim() }
                prompt = $Prompt
                outputSummary = $summary
                success = ($exitCode -eq 0)
                errorMessage = if ($exitCode -eq 0) { $null } else { $errorText }
                startedAtUtc = $startedAtUtc.ToString("O")
                completedAtUtc = $completedAtUtc.ToString("O")
            }

            Send-AgentExecutionReport -BaseUrl $AgentApiBaseUrl -TargetEmployeeId $EmployeeId -TargetEmployeeKey $EmployeeKey -Payload $payload
        }
    }

    exit $exitCode
}
