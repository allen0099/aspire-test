using Api;
using Database;
using Keycloak.AuthServices.Authentication;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<ApplicationDbContext>("sql");

// Add services to the container.
services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddApplicationOpenApi();
services.AddKeycloakWebApiAuthentication(
    configuration,
    options =>
    {
        options.Audience = "workspaces-client";
        if (builder.Environment.IsDevelopment()) options.RequireHttpsMetadata = false;
    }
);
services.AddAuthorization();

var app = builder.Build();

// 收集所有有 [Authorize] 的 endpoint
var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
foreach (var endpoint in endpointDataSource.Endpoints)
{
    var authorize = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
    var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
    var routePattern = endpoint is RouteEndpoint re ? re.RoutePattern.RawText : null;
    var httpMethods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods;
    if (authorize != null && routePattern != null && httpMethods != null)
        foreach (var method in httpMethods)
            AuthorizedEndpointsRegistry.AuthorizedEndpoints.TryAdd(
                (routePattern.TrimEnd('/'), method.ToUpperInvariant()), true);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.Purple);
        options.WithDarkMode(false);
        options.AddPreferredSecuritySchemes("keycloak-password")
            .AddPasswordFlow("keycloak-password", flow =>
            {
                flow.ClientId = "workspaces-client";
                flow.ClientSecret = "ze4SQDpbyBlB72kdTCTv8ecSWsJHf2Js";
                flow.Username = "test1";
                flow.Password = "test";
                flow.SelectedScopes = ["openid", "profile", "email"];
            });
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/hello", () => "Hello World!").RequireAuthorization();

app.MapControllers();

await app.RunAsync();

public static class AuthorizedEndpointsRegistry
{
    public static readonly ConcurrentDictionary<(string Path, string Method), bool> AuthorizedEndpoints = new();
}