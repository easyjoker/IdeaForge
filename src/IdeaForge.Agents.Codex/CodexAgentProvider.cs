using System.Diagnostics;
using System.Text;
using IdeaForge.Agents.Abstractions;

namespace IdeaForge.Agents.Codex;

public sealed class CodexAgentProvider : IAgentProvider
{
    private readonly CodexAgentOptions _options;

    public CodexAgentProvider(CodexAgentOptions? options = null)
    {
        _options = options ?? new CodexAgentOptions();
    }

    public string Name => "codex";

    public AgentProviderKind Kind => AgentProviderKind.Codex;

    public AgentProviderCapabilities Capabilities =>
        new(
            SupportsModelSelection: true,
            SupportsWorkingDirectory: true,
            SupportsStructuredOutput: true,
            SupportsNamedSessions: false);

    public async Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            throw new NotSupportedException("The current Codex CLI provider does not yet map request.SessionId to a resumable CLI session.");
        }

        var arguments = BuildArguments(request);
        var (fileName, processArguments) = ResolveCommandInvocation(arguments);
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = processArguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = ResolveWorkingDirectory(request)
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;
        var trimmedErrorOutput = string.IsNullOrWhiteSpace(standardError) ? null : standardError.Trim();

        return new AgentExecutionResult
        {
            Provider = Kind,
            Model = ResolveModel(request),
            ExitCode = process.ExitCode,
            Output = standardOutput.Trim(),
            ErrorOutput = process.ExitCode == 0 ? null : trimmedErrorOutput,
            OutputPath = request.OutputPath
        };
    }

    private string BuildArguments(AgentExecutionRequest request)
    {
        var arguments = new StringBuilder();

        arguments.Append("exec ");

        if (_options.DangerouslyBypassApprovalsAndSandbox)
        {
            arguments.Append("--dangerously-bypass-approvals-and-sandbox ");
        }
        else
        {
            arguments.Append("--sandbox ").Append(Escape(_options.Sandbox)).Append(' ');
        }

        if (_options.SkipGitRepoCheck)
        {
            arguments.Append("--skip-git-repo-check ");
        }

        if (_options.JsonOutput)
        {
            arguments.Append("--json ");
        }

        if (!string.IsNullOrWhiteSpace(request.OutputPath))
        {
            arguments.Append("--output-last-message ").Append(Escape(Path.GetFullPath(request.OutputPath))).Append(' ');
        }

        if (!string.IsNullOrWhiteSpace(request.SchemaPath))
        {
            arguments.Append("--output-schema ").Append(Escape(Path.GetFullPath(request.SchemaPath))).Append(' ');
        }

        foreach (var addDir in request.AdditionalDirectories.Where(static path => !string.IsNullOrWhiteSpace(path)))
        {
            arguments.Append("--add-dir ").Append(Escape(Path.GetFullPath(addDir))).Append(' ');
        }

        arguments.Append("--model ").Append(Escape(ResolveModel(request))).Append(' ');
        arguments.Append(Escape(request.Prompt));

        return arguments.ToString();
    }

    private string ResolveModel(AgentExecutionRequest request) =>
        string.IsNullOrWhiteSpace(request.Model) ? _options.DefaultModel : request.Model;

    private string ResolveWorkingDirectory(AgentExecutionRequest request) =>
        string.IsNullOrWhiteSpace(request.WorkingDirectory)
            ? Environment.CurrentDirectory
            : Path.GetFullPath(request.WorkingDirectory);

    private (string FileName, string Arguments) ResolveCommandInvocation(string arguments)
    {
        var commandPath = ResolveCommandPath();

        if (commandPath.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ||
            commandPath.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
        {
            return ("cmd.exe", $"/d /s /c \"\"{commandPath}\" {arguments}\"");
        }

        if (commandPath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
        {
            return ("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -File {Escape(commandPath)} {arguments}");
        }

        return (commandPath, arguments);
    }

    private string ResolveCommandPath()
    {
        if (!string.IsNullOrWhiteSpace(_options.CommandPath) &&
            !string.Equals(_options.CommandPath, "codex", StringComparison.OrdinalIgnoreCase))
        {
            return _options.CommandPath;
        }

        if (OperatingSystem.IsWindows())
        {
            foreach (var candidate in EnumeratePathCandidates("codex.cmd", "codex.ps1", "codex.exe", "codex"))
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return _options.CommandPath;
    }

    private static IEnumerable<string> EnumeratePathCandidates(params string[] fileNames)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            yield break;
        }

        foreach (var entry in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var fileName in fileNames)
            {
                yield return Path.Combine(entry, fileName);
            }
        }
    }

    private static string Escape(string value) =>
        $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
