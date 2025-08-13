using Keycloak.AuthServices.Authentication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddServiceDefaults();

// Add services to the container.
services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();
services.AddEndpointsApiExplorer();
services.AddAuthorization();

services.AddKeycloakWebApiAuthentication(
    configuration,
    options =>
    {
        options.RequireHttpsMetadata = false;
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/hello", () => "Hello World!").RequireAuthorization();

app.MapControllers();

await app.RunAsync();