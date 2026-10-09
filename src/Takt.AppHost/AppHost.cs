using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume("takt-identity-postgres-data");

var identityDatabase = postgres.AddDatabase("DefaultConnection", "takt_identity");
var peopleDatabase = postgres.AddDatabase("PeopleDefaultConnection", "takt_people");
var warehouseDatabase = postgres.AddDatabase("WarehouseDefaultConnection", "takt_warehouse");

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

var peopleMigration = builder.AddProject<Projects.Takt_Membership_API>("people-api-migrate-db", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ConnectionStrings__DefaultConnection", peopleDatabase.Resource.ConnectionStringExpression)
    .WithReference(peopleDatabase)
    .WaitFor(peopleDatabase)
    .WithArgs("--migrate-db")
    .WithExplicitStart();

var peopleApi = builder.AddProject<Projects.Takt_Membership_API>("people-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ConnectionStrings__DefaultConnection", peopleDatabase.Resource.ConnectionStringExpression)
    .WithReference(peopleDatabase)
    .WaitFor(peopleDatabase)
    .WithHttpEndpoint(port: 8082, targetPort: 8080);

var warehouseMigration = builder.AddProject<Projects.Takt_Warehouse_API>("warehouse-api-migrate-db", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ConnectionStrings__DefaultConnection", warehouseDatabase.Resource.ConnectionStringExpression)
    .WithReference(warehouseDatabase)
    .WaitFor(warehouseDatabase)
    .WithArgs("--migrate-db")
    .WithExplicitStart();

var warehouseApi = builder.AddProject<Projects.Takt_Warehouse_API>("warehouse-api", options =>
    {
        options.ExcludeLaunchProfile = true;
        options.ExcludeKestrelEndpoints = true;
    })
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ConnectionStrings__DefaultConnection", warehouseDatabase.Resource.ConnectionStringExpression)
    .WithReference(warehouseDatabase)
    .WaitFor(warehouseDatabase)
    .WithHttpEndpoint(port: 8083, targetPort: 8080);

var identityHttpEndpoint = identityApi.GetEndpoint("http");
var peopleHttpEndpoint = peopleApi.GetEndpoint("http");
var warehouseHttpEndpoint = warehouseApi.GetEndpoint("http");

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

        yarp.AddRoute("/people/{**catch-all}", peopleHttpEndpoint)
            .WithTransformPathRemovePrefix("/people")
            .WithTransformRequestHeader("X-Forwarded-Prefix", "/people", append: false);

        yarp.AddRoute("/club/{**catch-all}", peopleHttpEndpoint)
            .WithTransformPathRemovePrefix("/club")
            .WithTransformRequestHeader("X-Forwarded-Prefix", "/club", append: false);

        yarp.AddRoute("/warehouse/{**catch-all}", warehouseHttpEndpoint)
            .WithTransformPathRemovePrefix("/warehouse")
            .WithTransformRequestHeader("X-Forwarded-Prefix", "/warehouse", append: false);

        yarp.AddRoute(frontendHttpEndpoint);
    });

builder.Build().Run();