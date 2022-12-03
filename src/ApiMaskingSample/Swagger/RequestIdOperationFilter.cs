using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiMaskingSample.Swagger;

public class RequestIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Request-Id",
            In = ParameterLocation.Header,
            Description = "Required request Id in header for each request, supposed to be unique for each request",
            AllowEmptyValue = false,
            Schema = new OpenApiSchema { Type = "string" },
            Example = new OpenApiString(Guid.NewGuid().ToString()),
            Required = true
        });
    }
}