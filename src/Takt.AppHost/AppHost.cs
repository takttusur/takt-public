using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume("takt-identity-postgres-data");

var identityDatabase = postgres.AddDatabase("DefaultConnection", "takt_identity");

var identityMigration = builder.AddProject<Projects.Takt_Identity_API>("identity-api-migrate-db", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Tokens__PrivateKeyPemPath", "./keys/dev-jwt-private.pem")
    .WithReference(identityDatabase)
    .WaitFor(identityDatabase)
    .WithArgs("--migrate-db")
    .WithExplicitStart();

var identityBootstrapAdmin = builder.AddProject<Projects.Takt_Identity_API>("identity-api-bootstrap-admin", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Tokens__PrivateKeyPemPath", "./keys/dev-jwt-private.pem")
    .WithReference(identityDatabase)
    .WaitFor(identityDatabase)
    .WithArgs("--bootstrap-admin")
    .WithExplicitStart();

var identityApi = builder.AddProject<Projects.Takt_Identity_API>("identity-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("Tokens__PrivateKeyPemPath", "./keys/dev-jwt-private.pem")
    .WithReference(identityDatabase)
    .WaitFor(identityDatabase)
    .WithHttpEndpoint(port: 8081, targetPort: 8080);

var identityHttpEndpoint = identityApi.GetEndpoint("http");

var frontend = builder
    .AddDockerfile("frontend", "../..", "src/takt-spa/Dockerfile.dev", "dev")
    .WithBindMount("../../src/takt-spa", "/app", isReadOnly: false)
    .WithHttpEndpoint(targetPort: 5173)
    .WithEnvironment("VITE_HMR_CLIENT_PORT", "3333")
    .WithEnvironment("VITE_HMR_HOST", "localhost")
    .WithEnvironment("CHOKIDAR_USEPOLLING", "true");

var frontendHttpEndpoint = frontend.GetEndpoint("http");

builder
    .AddYarp("gateway")
    .WithHostPort(3333)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/identity/{**catch-all}", identityHttpEndpoint)
            .WithTransformPathRemovePrefix("/identity")
            .WithTransformRequestHeader("X-Forwarded-Prefix", "/identity", append: false);

        yarp.AddRoute(frontendHttpEndpoint);
    });

builder.Build().Run();