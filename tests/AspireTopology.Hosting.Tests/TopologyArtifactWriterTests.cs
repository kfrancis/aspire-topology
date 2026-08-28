using AspireTopology.Hosting.Extraction;
using AspireTopology.Serialization;

namespace AspireTopology.Hosting.Tests;

public class TopologyArtifactWriterTests
{
    [Test]
    public async Task WriteAsync_WritesTopologyAndRendererOutput()
    {
        var directory = Directory.CreateTempSubdirectory("aspire-topology-tests");

        try
        {
            var options = new TopologyDiagramOptions { OutputPath = "artifacts/topology", DocumentName = "sample" };
            var topology = new AspireTopologyExtractor(options).Extract(SampleApplication.Build());

            var written = await new TopologyArtifactWriter(options).WriteAsync(topology, directory.FullName);

            var topologyPath = Path.Combine(directory.FullName, "artifacts", "topology", "topology.json");
            var isoflowPath = Path.Combine(directory.FullName, "artifacts", "topology", "topology.isoflow.json");

            await Assert.That(written.Count).IsEqualTo(2);
            await Assert.That(File.Exists(topologyPath)).IsTrue();
            await Assert.That(File.Exists(isoflowPath)).IsTrue();

            var restored = TopologyJson.Deserialize(await File.ReadAllTextAsync(topologyPath));
            await Assert.That(restored.Nodes.Count).IsEqualTo(topology.Nodes.Count);

            var isoflow = await File.ReadAllTextAsync(isoflowPath);
            await Assert.That(isoflow.Contains("\"views\"", StringComparison.Ordinal)).IsTrue();
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Test]
    public async Task WriteAsync_HonoursAbsoluteOutputPaths()
    {
        var directory = Directory.CreateTempSubdirectory("aspire-topology-tests");

        try
        {
            var target = Path.Combine(directory.FullName, "docs", "architecture");
            var options = new TopologyDiagramOptions { OutputPath = target, DocumentName = "sample" };
            var topology = new AspireTopologyExtractor(options).Extract(SampleApplication.Build());

            await new TopologyArtifactWriter(options).WriteAsync(topology, baseDirectory: directory.FullName);

            await Assert.That(File.Exists(Path.Combine(target, "topology.json"))).IsTrue();
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
