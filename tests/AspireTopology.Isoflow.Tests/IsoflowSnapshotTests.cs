using AspireTopology.Model;

namespace AspireTopology.Isoflow.Tests;

/// <summary>
/// Golden tests over the rendered Isoflow JSON. These catch renderer regressions that structural
/// assertions miss, such as a tile shifting or an icon changing.
/// </summary>
public class IsoflowSnapshotTests
{
    [Test]
    [Arguments("simple-app.isoflow.json")]
    [Arguments("postgres-redis.isoflow.json")]
    public async Task Render_MatchesSnapshot(string snapshotName)
    {
        var topology = snapshotName switch
        {
            "simple-app.isoflow.json" => SampleTopologies.SimpleApp(),
            "postgres-redis.isoflow.json" => SampleTopologies.PostgresAndRedis(),
            _ => throw new ArgumentOutOfRangeException(nameof(snapshotName)),
        };

        var rendered = SnapshotFile.Normalize(new IsoflowTopologyRenderer().RenderToString(topology));

        if (SnapshotFile.ShouldUpdate)
        {
            SnapshotFile.Write(snapshotName, rendered);
        }

        await Assert.That(rendered).IsEqualTo(SnapshotFile.Read(snapshotName));
    }

    [Test]
    public async Task Render_IsStableAcrossCalls()
    {
        var renderer = new IsoflowTopologyRenderer();
        var topology = SampleTopologies.PostgresAndRedis();

        await Assert.That(renderer.RenderToString(topology)).IsEqualTo(renderer.RenderToString(topology));
    }

    [Test]
    public async Task Render_HandlesAnEmptyTopology()
    {
        var document = new IsoflowTopologyRenderer().Map(TopologyDocument.Empty("empty"));

        await Assert.That(document.Items.Count).IsEqualTo(0);
        await Assert.That(document.Views.Single().Items.Count).IsEqualTo(0);
    }
}
