[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0, ValueFromPipeline = $true, ValueFromPipelineByPropertyName = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Prompt,

    [Parameter()]
    [string]$Agent,

    [Parameter()]
    [string]$Model = "gpt-5.4",

    [Parameter()]
    [string]$ModelRegistryPath,

    [Parameter()]
    [string[]]$DenyTool,

    [Parameter()]
    [string[]]$AddDir,

    [Parameter()]
    [string[]]$SecretEnvVars,

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
    [Alias("SharePath")]
    [string]$TranscriptPath,

    [Parameter()]
    [string]$WorkingDirectory,

    [Parameter()]
    [ValidateSet("text", "json")]
    [string]$OutputFormat = "text",

    [Parameter()]
    [switch]$Silent,

    [Parameter()]
    [switch]$NoAskUser,

    [Parameter()]
    [switch]$ShareGist,

    [Parameter()]
    [bool]$AllowAllTools = $true,

    [Parameter()]
    [bool]$AllowAllUrls = $true,

    [Parameter()]
    [bool]$AllowAllPaths = $true
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

    function Get-CopilotSessionId {
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
                if ($json.type -eq "result" -and -not [string]::IsNullOrWhiteSpace($json.sessionId)) {
                    return $json.sessionId
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
    $copilotCommand = Get-Command copilot -ErrorAction SilentlyContinue
    if (-not $copilotCommand) {
        throw "The 'copilot' command was not found in PATH."
    }

    $isByokMode = -not [string]::IsNullOrWhiteSpace($env:COPILOT_PROVIDER_BASE_URL)

    if ($Model -and -not $isByokMode) {
        $resolvedModelRegistryPath = $ModelRegistryPath
        if (-not $resolvedModelRegistryPath) {
            $resolvedModelRegistryPath = Join-Path -Path $PSScriptRoot -ChildPath "copilot-models.json"
        }

        $resolvedModelRegistryPath = [System.IO.Path]::GetFullPath($resolvedModelRegistryPath)

        if (-not (Test-Path -LiteralPath $resolvedModelRegistryPath)) {
            throw "Copilot model registry not found: $resolvedModelRegistryPath"
        }

        $modelRegistry = Get-Content -LiteralPath $resolvedModelRegistryPath -Raw | ConvertFrom-Json
        $allowedModels = @($modelRegistry.models | ForEach-Object { $_.id })

        if ($allowedModels -notcontains $Model) {
            throw "Model '$Model' was not found in the Copilot model registry: $resolvedModelRegistryPath"
        }
    }

    $args = @("-p", $Prompt)

    if ($Silent) {
        $args += "-s"
    }

    if ($Agent) {
        $args += "--agent=$Agent"
    }

    if ($Model) {
        $args += "--model=$Model"
    }

    if ($OutputFormat) {
        $args += "--output-format=$OutputFormat"
    }

    if ($NoAskUser) {
        $args += "--no-ask-user"
    }

    if ($AllowAllTools) {
        $args += "--allow-all-tools"
    }

    if ($AllowAllUrls) {
        $args += "--allow-all-urls"
    }

    if ($AllowAllPaths) {
        $args += "--allow-all-paths"
    }

    if ($DenyTool) {
        $joinedDenyTool = ($DenyTool | Where-Object { $_ }) -join ", "
        if ($joinedDenyTool) {
            $args += "--deny-tool=$joinedDenyTool"
        }
    }

    if ($AddDir) {
        foreach ($dir in ($AddDir | Where-Object { $_ })) {
            $resolvedDir = [System.IO.Path]::GetFullPath($dir)
            $args += "--add-dir=$resolvedDir"
        }
    }

    if ($SecretEnvVars) {
        $joinedSecretEnvVars = ($SecretEnvVars | Where-Object { $_ }) -join ", "
        if ($joinedSecretEnvVars) {
            $args += "--secret-env-vars=$joinedSecretEnvVars"
        }
    }

    if ($TranscriptPath) {
        $args += "--share=$TranscriptPath"
    }

    if ($ShareGist) {
        $args += "--share-gist"
    }

    $exitCode = 0
    $resolvedOutputPath = $null
    $commandOutput = @()
    $errorText = $null
    $startedAtUtc = [DateTimeOffset]::UtcNow

    if ($OutputPath) {
        $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
    }

    try {
        if ($WorkingDirectory) {
            $resolvedWorkingDirectory = [System.IO.Path]::GetFullPath($WorkingDirectory)
            Push-Location -LiteralPath $resolvedWorkingDirectory
            try {
                & $copilotCommand.Source @args 2>&1 | Tee-Object -Variable commandOutput
                $exitCode = $LASTEXITCODE
            }
            finally {
                Pop-Location
            }
        }
        else {
            & $copilotCommand.Source @args 2>&1 | Tee-Object -Variable commandOutput
            $exitCode = $LASTEXITCODE
        }
    }
    catch {
        $exitCode = 1
        $errorText = $_.Exception.Message
        throw
    }
    finally {
        $completedAtUtc = [DateTimeOffset]::UtcNow

        if ($resolvedOutputPath) {
            (($commandOutput | ForEach-Object { "$_" }) -join [Environment]::NewLine) | Set-Content -Path $resolvedOutputPath
        }

        if ($AgentApiBaseUrl -and ($EmployeeId -or $EmployeeKey)) {
            $resolvedSessionId = $SessionId
            if ([string]::IsNullOrWhiteSpace($resolvedSessionId) -and $OutputFormat -eq "json") {
                $resolvedSessionId = Get-CopilotSessionId -CommandOutput $commandOutput
            }

            $summary = Get-ExecutionSummary -PreferredSummary $ExecutionSummary -CommandOutput $commandOutput -FallbackError $errorText
            $payload = @{
                provider = 1
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
