using IdeaForge.Agents.Abstractions;
using GitHub.Copilot.SDK;

namespace IdeaForge.Agents.Copilot;

public sealed class CopilotAgentProvider : IAgentProvider
{
    private readonly CopilotAgentOptions _options;

    public CopilotAgentProvider(CopilotAgentOptions? options = null)
    {
        _options = options ?? new CopilotAgentOptions();
    }

    public string Name => "copilot";

    public AgentProviderKind Kind => AgentProviderKind.Copilot;

    public AgentProviderCapabilities Capabilities =>
        new(
            SupportsModelSelection: true,
            SupportsWorkingDirectory: true,
            SupportsStructuredOutput: false,
            SupportsNamedSessions: true);

    public async Task<AgentExecutionResult> ExecuteAsync(
        AgentExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(request));
        }

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var outputChunks = new List<string>();
        string? errorOutput = null;

        await using var client = new CopilotClient(new CopilotClientOptions
        {
            CliPath = _options.CliPath,
            Cwd = ResolveWorkingDirectory(request)
        });

        await client.StartAsync(cancellationToken);

        await using var session = string.IsNullOrWhiteSpace(request.SessionId)
            ? await client.CreateSessionAsync(CreateSessionConfig(request), cancellationToken)
            : await client.ResumeSessionAsync(request.SessionId, CreateResumeSessionConfig(), cancellationToken);

        using var subscription = session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageEvent messageEvent when !string.IsNullOrWhiteSpace(messageEvent.Data.Content):
                    outputChunks.Add(messageEvent.Data.Content);
                    break;
                case SessionErrorEvent sessionErrorEvent:
                    errorOutput = sessionErrorEvent.Data.Message;
                    completion.TrySetResult();
                    break;
                case SessionIdleEvent:
                    completion.TrySetResult();
                    break;
            }
        });

        await session.SendAsync(new MessageOptions { Prompt = request.Prompt });
        using var _ = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        await completion.Task.WaitAsync(cancellationToken);

        var output = string.Join(Environment.NewLine, outputChunks.Where(static chunk => !string.IsNullOrWhiteSpace(chunk))).Trim();

        if (!string.IsNullOrWhiteSpace(request.OutputPath))
        {
            var resolvedOutputPath = Path.GetFullPath(request.OutputPath);
            var parentDirectory = Path.GetDirectoryName(resolvedOutputPath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            await File.WriteAllTextAsync(resolvedOutputPath, output, cancellationToken);
        }

        return new AgentExecutionResult
        {
            Provider = Kind,
            Model = ResolveModel(request),
            SessionId = session.SessionId,
            ExitCode = string.IsNullOrWhiteSpace(errorOutput) ? 0 : 1,
            Output = output,
            ErrorOutput = errorOutput,
            OutputPath = request.OutputPath
        };
    }

    private SessionConfig CreateSessionConfig(AgentExecutionRequest request) =>
        new()
        {
            SessionId = request.SessionId,
            Model = ResolveModel(request),
            OnPermissionRequest = ResolvePermissionHandler()
        };

    private ResumeSessionConfig CreateResumeSessionConfig() =>
        new()
        {
            OnPermissionRequest = ResolvePermissionHandler()
        };

    private PermissionRequestHandler ResolvePermissionHandler()
    {
        if (_options.ApproveAllPermissions)
        {
            return PermissionHandler.ApproveAll;
        }

        return static (_, _) => Task.FromResult(new PermissionRequestResult
        {
            Kind = PermissionRequestResultKind.DeniedCouldNotRequestFromUser
        });
    }

    private string ResolveModel(AgentExecutionRequest request) =>
        string.IsNullOrWhiteSpace(request.Model) ? _options.DefaultModel : request.Model;

    private static string? ResolveWorkingDirectory(AgentExecutionRequest request) =>
        string.IsNullOrWhiteSpace(request.WorkingDirectory)
            ? null
            : Path.GetFullPath(request.WorkingDirectory);
}
