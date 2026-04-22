using IdeaForge.Application;
using IdeaForge.Application.Agents;
using IdeaForge.Agents;
using IdeaForge.Infrastructure;
using IdeaForge.Infrastructure.Persistence;
using IdeaForge.WebApi.Swagger;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    foreach (var assembly in new[]
             {
                 typeof(Program).Assembly,
                 typeof(CreateAgentProfileRequest).Assembly,
                 typeof(IdeaForge.Agents.Abstractions.AgentProviderKind).Assembly
             })
    {
        var xmlFile = $"{assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: assembly == typeof(Program).Assembly);
        }
    }

    options.SchemaFilter<EnumSchemaDescriptionFilter>();
});
builder.Services.AddIdeaForgeApplication();
builder.Services.AddIdeaForgeAgents();
builder.Services.AddIdeaForgeInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.GetRequiredService<IPostgreSqlInitializer>().InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
