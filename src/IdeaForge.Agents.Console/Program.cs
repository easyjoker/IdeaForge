using IdeaForge.Agents;
using IdeaForge.Agents.Abstractions;
using Microsoft.Extensions.DependencyInjection;

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
{
    PrintHelp();
    return;
}

var options = ParseArguments(args);

var services = new ServiceCollection();
services.AddIdeaForgeAgents();

using var serviceProvider = services.BuildServiceProvider();
var registry = serviceProvider.GetRequiredService<AgentProviderRegistry>();
var provider = registry.GetRequiredProvider(options.Provider);

var request = new AgentExecutionRequest
{
    Prompt = options.Prompt ?? throw new InvalidOperationException("A prompt is required."),
    Model = options.Model,
    SessionId = options.SessionId,
    WorkingDirectory = options.WorkingDirectory,
    OutputPath = options.OutputPath
};

var result = await provider.ExecuteAsync(request);

Console.WriteLine($"Provider: {provider.Name}");
Console.WriteLine($"Model: {result.Model ?? "(default)"}");
Console.WriteLine($"ExitCode: {result.ExitCode}");
if (!string.IsNullOrWhiteSpace(result.SessionId))
{
    Console.WriteLine($"SessionId: {result.SessionId}");
}

Console.WriteLine();
Console.WriteLine(result.Output);

if (!string.IsNullOrWhiteSpace(result.ErrorOutput))
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Error:");
    Console.Error.WriteLine(result.ErrorOutput);
}

Environment.ExitCode = result.ExitCode;

static ConsoleOptions ParseArguments(string[] args)
{
    var options = new ConsoleOptions();

    for (var index = 0; index < args.Length; index++)
    {
        var current = args[index];

        switch (current)
        {
            case "--provider":
            case "-p":
                options.Provider = ParseProvider(GetNextValue(args, ref index, current));
                break;
            case "--prompt":
                options.Prompt = GetNextValue(args, ref index, current);
                break;
            case "--model":
                options.Model = GetNextValue(args, ref index, current);
                break;
            case "--session":
                options.SessionId = GetNextValue(args, ref index, current);
                break;
            case "--cwd":
                options.WorkingDirectory = GetNextValue(args, ref index, current);
                break;
            case "--output":
                options.OutputPath = GetNextValue(args, ref index, current);
                break;
            default:
                throw new ArgumentException($"Unknown argument: {current}");
        }
    }

    if (string.IsNullOrWhiteSpace(options.Prompt))
    {
        throw new ArgumentException("Missing required argument: --prompt");
    }

    return options;
}

static string GetNextValue(string[] args, ref int index, string optionName)
{
    if (index + 1 >= args.Length)
    {
        throw new ArgumentException($"Missing value for {optionName}");
    }

    index++;
    return args[index];
}

static AgentProviderKind ParseProvider(string value) =>
    value.ToLowerInvariant() switch
    {
        "copilot" => AgentProviderKind.Copilot,
        "codex" => AgentProviderKind.Codex,
        _ => throw new ArgumentException($"Unsupported provider: {value}")
    };

static void PrintHelp()
{
    Console.WriteLine("IdeaForge Agents Console");
    Console.WriteLine();
    Console.WriteLine("Required:");
    Console.WriteLine("  --provider <copilot|codex>");
    Console.WriteLine("  --prompt <text>");
    Console.WriteLine();
    Console.WriteLine("Optional:");
    Console.WriteLine("  --model <model>");
    Console.WriteLine("  --session <session-id>");
    Console.WriteLine("  --cwd <working-directory>");
    Console.WriteLine("  --output <file-path>");
}

sealed class ConsoleOptions
{
    public AgentProviderKind Provider { get; set; } = AgentProviderKind.Copilot;

    public string? Prompt { get; set; }

    public string? Model { get; set; }

    public string? SessionId { get; set; }

    public string? WorkingDirectory { get; set; }

    public string? OutputPath { get; set; }
}
