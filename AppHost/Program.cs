var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder
    .AddKeycloakContainer("keycloak")
    // .WithBindMount("./Keycloak/Themes","/opt/keycloak/themes")
    .WithBindMount("./Keycloak/Providers", "/opt/keycloak/providers")
    // .WithImport("./Keycloak/Configurations/realm-export.json")
    .WithImport("./Keycloak/Configurations/Test-realm.json")
    .WithDataVolume()
    .WithImage("keycloak/keycloak", "26.3.2");

var realm = keycloak.AddRealm("Test");

var sql = builder.AddSqlServer("sql");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(realm)
    .WithReference(sql);

builder.AddNpmApp("webapp", "../WebApp", "dev")
    .WithReference(api)
    .WithHttpEndpoint(5170, targetPort: 5173);

builder.Build().Run();