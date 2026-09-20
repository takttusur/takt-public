var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder
    .AddDockerfile("webapi", "../Takt.Services.WebApi", "Dockerfile", "final")
    .WithHttpEndpoint(targetPort: 8080)
    .WithExternalHttpEndpoints();

builder
    .AddDockerfile("frontend", "../../frontend", "Dockerfile.dev", "dev")
    .WithBindMount("../../frontend", "/app", false)
    .WithHttpEndpoint(targetPort: 5173)
    .WithEnvironment("VITE_API_PROXY_TARGET", webApi.GetEndpoint("http"))
    .WithEnvironment("CHOKIDAR_USEPOLLING", "true")
    .WaitFor(webApi)
    .WithExternalHttpEndpoints();

builder.Build().Run();