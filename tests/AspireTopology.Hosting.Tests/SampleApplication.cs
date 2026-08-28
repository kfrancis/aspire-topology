using Aspire.Hosting.ApplicationModel;

namespace AspireTopology.Hosting.Tests;

/// <summary>
/// Builds the application model the sample AppHost produces: a web front end, an API, a Postgres
/// server with a database inside it, and a Redis cache.
/// </summary>
internal static class SampleApplication
{
    public static DistributedApplicationModel Build()
    {
        var postgres = new PostgresResource("postgres").WithImage("postgres", "17.2");
        var appDb = new PostgresDatabaseResource("appdb", postgres);
        var cache = new RedisResource("cache").WithImage("redis", "7.4");

        var api = new ProjectResource("api")
            .WithEndpoint("http", "http", targetPort: 8080)
            .WithReferenceTo(appDb)
            .WithReferenceTo(cache)
            .WaitingFor(appDb);

        var web = new ProjectResource("web")
            .WithEndpoint("https", "https", external: true)
            .WithReferenceTo(api)
            .WaitingFor(api);

        // Aspire adds a Parent relationship annotation alongside IResourceWithParent.
        appDb.Annotations.Add(new ResourceRelationshipAnnotation(postgres, "Parent"));

        return new DistributedApplicationModel([postgres, appDb, cache, api, web]);
    }
}
