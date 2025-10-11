using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace App.Api.Extensions;

public sealed class JwtSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document,
                               OpenApiDocumentTransformerContext context,
                               CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes["AppBearer"] = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Paste: Bearer {token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "AppBearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    }
}