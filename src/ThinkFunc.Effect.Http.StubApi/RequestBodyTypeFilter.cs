using LanguageExt;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using static LanguageExt.Prelude;

namespace ThinkFunc.Effect.Http.StubApi;

public class RequestBodyTypeFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requiredScopes = context.MethodInfo?
           .GetCustomAttributes(true)
           .OfType<ConsumesAttribute>()
           .Select(attr => (attr.ContentTypes, ((IAcceptsMetadata)attr).RequestType))
           .Distinct();

        if (requiredScopes is not null)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = requiredScopes.ToDictionary(
                x => x.ContentTypes.First(),
                x => new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        x.RequestType,
                        context.SchemaRepository)
                })
            };
        }
    }
}
