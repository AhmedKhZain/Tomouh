using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tomouh.API.Filters;

public class IdempotencyHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttribute = context.MethodInfo.GetCustomAttributes(true)
            .Any(a => a.GetType() == typeof(RequireIdempotencyHeaderAttribute));

        if (!hasAttribute) return;

        if (operation.Parameters is null)
        {
            operation.Parameters = new List<IOpenApiParameter>();
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Idempotency-Key",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Unique request identifier (GUID) to prevent duplicate processing.",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            }
        });
    }
}
