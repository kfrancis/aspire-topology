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

// Refreshes the artifacts on start and lists the viewer in the dashboard, both by default.
// Also registers the "topology" pipeline step, for: aspire do topology
builder.AddTopologyDiagram(options =>
{
    // Only overridden here so the sample writes to the repository root rather than next to itself.
    options.OutputPath = "../../../artifacts/topology";
});

builder.Build().Run();
