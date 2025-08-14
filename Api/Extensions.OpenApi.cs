using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;

namespace Api;

using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.OpenApi.Models;

public static class ExtensionsOpenApi
{
    internal sealed class BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        private const string SchemeName = "Bearer";

        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme => authScheme.Name == SchemeName))
            {
                var scheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes[SchemeName] = scheme;
            }
        }
    }

    internal sealed class KeycloakSecuritySchemeTransformer(
        IConfiguration configuration,
        IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        private const string SchemeName = "keycloak-oidc";

        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.Any(authScheme =>
                    authScheme.Name == SchemeName)) return; // No need to add the security scheme if it already exists

            var keycloakOptions =
                configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;

            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OpenIdConnect,
                Description = "Keycloak OpenID Connect discovery endpoint",
                Name = SchemeName,
                OpenIdConnectUrl = new Uri(keycloakOptions.OpenIdConnectUrl!)
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes[SchemeName] = scheme;
        }
    }

    internal sealed class KeycloakPasswordSchemeTransformer(
        IConfiguration configuration,
        IAuthenticationSchemeProvider authenticationSchemeProvider
    ) : IOpenApiDocumentTransformer
    {
        private const string SchemeName = "keycloak-password";

        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (authenticationSchemes.All(authScheme => authScheme.Name != SchemeName))
            {
                var keycloakOptions =
                    configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>()!;

                // Password Flow + 預設 scopes
                var scheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Keycloak password flow authentication",
                    Name = SchemeName,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri(
                                $"{keycloakOptions.KeycloakUrlRealm}{KeycloakConstants.TokenEndpointPath}"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID Connect scope" },
                                { "profile", "Basic profile information" },
                                { "email", "Email address" }
                            }
                        }
                    }
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes[SchemeName] = scheme;
            }

            // 自動幫特定標籤加 security requirement
            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SchemeName
                        }
                    },
                    ["openid", "profile", "email"]
                }
            };

            // 掃描每個 path operation 是否在 AuthorizedEndpointsRegistry 中
            foreach (var (path, pathItem) in document.Paths)
            foreach (var op in pathItem.Operations)
            {
                var method = op.Key.ToString().ToUpperInvariant();
                var normalizedPath = path.TrimEnd('/');
                if (AuthorizedEndpointsRegistry.AuthorizedEndpoints.ContainsKey((normalizedPath, method)))
                {
                    op.Value.Security ??= new List<OpenApiSecurityRequirement>();
                    op.Value.Security.Add(securityRequirement);
                }
            }
        }
    }

    internal sealed class InternalServerErrorResponseTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            var basicJsonSchema = new OpenApiSchema
            {
                Type = "object",
                Properties =
                {
                    ["message"] = new OpenApiSchema
                    {
                        Type = "string",
                        Description = "A message describing the error."
                    },
                    ["date"] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "date",
                        Description = "The date in ISO 8601 format."
                    }
                }
            };
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal server error",
                Content =
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = basicJsonSchema
                    }
                }
            });
            return Task.CompletedTask;
        }
    }

    public static void AddApplicationOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddDocumentTransformer<KeycloakSecuritySchemeTransformer>();
            options.AddDocumentTransformer<KeycloakPasswordSchemeTransformer>();

            options.AddOperationTransformer<InternalServerErrorResponseTransformer>();
        });
        services.AddEndpointsApiExplorer();
    }
}