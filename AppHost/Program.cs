var builder = DistributedApplication.CreateBuilder(args);

var cache = builder
    .AddValkey("cache");

var keycloak = builder
    .AddKeycloakContainer("keycloak")
    // .WithBindMount("./Keycloak/Themes","/opt/keycloak/themes")
    .WithBindMount("./Keycloak/Providers", "/opt/keycloak/providers")
    .WithImage("keycloak/keycloak", "26.3.2")
    // .WithDataVolume()
    .WithImport("./Keycloak/Configurations/realm-export.json");

var realm = keycloak.AddRealm("realm-test");

builder.AddProject<Projects.Api>("api")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(realm)
    .WithReference(cache);

builder.Build().Run();