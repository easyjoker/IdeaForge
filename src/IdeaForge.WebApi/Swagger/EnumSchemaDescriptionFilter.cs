using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IdeaForge.WebApi.Swagger;

public sealed class EnumSchemaDescriptionFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!enumType.IsEnum)
        {
            return;
        }

        var values = Enum.GetValues(enumType)
            .Cast<object>()
            .Select(value => new
            {
                Name = Enum.GetName(enumType, value) ?? value.ToString() ?? string.Empty,
                NumericValue = Convert.ToInt32(value)
            })
            .ToArray();

        var enumDescription = string.Join(Environment.NewLine, values.Select(value => $"- `{value.NumericValue}` = `{value.Name}`"));
        var existingDescription = string.IsNullOrWhiteSpace(schema.Description)
            ? string.Empty
            : $"{schema.Description.Trim()}{Environment.NewLine}{Environment.NewLine}";

        schema.Description = $"{existingDescription}Available values:{Environment.NewLine}{enumDescription}";
    }
}
