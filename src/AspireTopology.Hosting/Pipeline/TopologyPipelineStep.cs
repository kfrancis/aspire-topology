using Aspire.Hosting;
using AspireTopology.Hosting.Extraction;
using Microsoft.Extensions.Logging;

namespace AspireTopology.Hosting.Pipeline;

/// <summary>
/// Registers the <c>topology</c> step that <c>aspire do topology</c> runs.
/// </summary>
/// <remarks>
/// Aspire marks its pipeline API experimental (ASPIREPIPELINES001). Every use of it in
/// AspireTopology lives in this file, so a breaking change in a future Aspire release is a change
/// to one small area rather than a change throughout the codebase.
/// </remarks>
public static class TopologyPipelineStep
{
    /// <summary>The pipeline step name, and therefore the argument to <c>aspire do</c>.</summary>
    public const string StepName = "topology";

    /// <summary>Adds the topology step to the AppHost pipeline.</summary>
    /// <param name="builder">The AppHost builder.</param>
    /// <param name="options">Options describing what to extract and where to write it.</param>
    /// <param name="extractor">
    /// The extractor to use. A default <see cref="AspireTopologyExtractor"/> is created when
    /// <see langword="null"/>.
    /// </param>
#pragma warning disable ASPIREPIPELINES001 // Aspire pipeline API is experimental; isolated here on purpose.
    public static void Add(
        IDistributedApplicationBuilder builder,
        TopologyDiagramOptions options,
        ITopologyExtractor? extractor = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        var appHostDirectory = builder.AppHostDirectory;
        var effectiveExtractor = extractor ?? new AspireTopologyExtractor(options);
        var writer = new TopologyArtifactWriter(options);

        builder.Pipeline.AddStep(StepName, async context =>
        {
            var topology = effectiveExtractor.Extract(context.Model);
            var written = await writer.WriteAsync(topology, appHostDirectory, context.CancellationToken)
                .ConfigureAwait(false);

            foreach (var path in written)
            {
                context.Logger.LogInformation("Wrote {Path}", path);
            }
        });
    }
#pragma warning restore ASPIREPIPELINES001
}
