using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace certifyab.Extensions
{
    public static class SwaggerSetup
    {
        public static void AddSwaggerConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Name = "X-API-Key",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Enter your API key."
                });

                options.OperationFilter<ApiKeyOperationFilter>();
            });
        }

        public sealed class ApiKeyRequiredMetadata
        {
        }

        public class ApiKeyOperationFilter : IOperationFilter
        {
            public void Apply(
                OpenApiOperation operation,
                OperationFilterContext context)
            {
                var requiresApiKey = context.ApiDescription
                    .ActionDescriptor
                    .EndpointMetadata
                    .OfType<ApiKeyRequiredMetadata>()
                    .Any();

                if (!requiresApiKey)
                    return;

                operation.Security ??= [];

                operation.Security.Add(
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(
                            "ApiKey",
                            context.Document)] = []
                    });
            }
        }
    }
}