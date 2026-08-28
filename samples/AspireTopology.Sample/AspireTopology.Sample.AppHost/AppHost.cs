var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var appDb = postgres.AddDatabase("appdb");

var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.AspireTopology_Sample_Api>("api")
    .WithReference(appDb)
    .WithReference(cache)
    .WaitFor(appDb)
    .WithTopologyMetadata(x =>
    {
        x.DisplayName = "Public API";
        x.Group = "Backend";
    });

builder.AddProject<Projects.AspireTopology_Sample_Web>("web")
    .WithReference(api)
    .WaitFor(api)
    .WithTopologyMetadata(x => x.Group = "Frontend");

// Registers the "topology" pipeline step. Run: aspire do topology
builder.AddTopologyDiagram(options =>
{
    options.OutputPath = "../../../artifacts/topology";

    // Also refresh the artifacts whenever the AppHost starts.
    options.GenerateOnStart = true;
});

builder.Build().Run();
