var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder
    .AddDockerfile("webapi", "../..", "src/Takt.Services.WebApi/Dockerfile", "final")
    .WithHttpEndpoint(targetPort: 8080);

var frontend = builder
    .AddDockerfile("frontend", "../..", "src/takt-spa/Dockerfile.dev", "dev")
    .WithBindMount("../../src/takt-spa", "/app", false)
    .WithHttpEndpoint(targetPort: 5173)
    .WithEnvironment("VITE_API_PROXY_TARGET", webApi.GetEndpoint("http"))
    .WithEnvironment("VITE_HMR_CLIENT_PORT", "8080")
    .WithEnvironment("VITE_HMR_HOST", "localhost")
    .WithEnvironment("CHOKIDAR_USEPOLLING", "true")
    .WaitFor(webApi);

builder
    .AddDockerfile("proxy", "../..", "docker/proxy/Dockerfile", "final")
    .WithHttpEndpoint(port: 3333, targetPort: 80)
    .WaitFor(webApi)
    .WaitFor(frontend)
    .WithExternalHttpEndpoints();

builder.Build().Run();