using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Extraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspireTopology.Hosting.Startup;

/// <summary>
/// Regenerates the topology every time the AppHost starts.
/// </summary>
/// <remarks>
/// This is off by default. <c>aspire do topology</c> is the deliberate way to produce artifacts;
/// generating on every F5 is a convenience for people who would rather never think about the
/// command. The two paths share the same extractor and writer, so they produce identical files.
/// </remarks>
public static class TopologyStartupGeneration
{
    /// <summary>Name of the logger the startup generation writes to.</summary>
    public const string LoggerName = "AspireTopology";

    /// <summary>Subscribes topology generation to the AppHost start event.</summary>
    /// <param name="builder">The AppHost builder.</param>
    /// <param name="options">Options describing what to extract and where to write it.</param>
    /// <param name="extractor">
    /// The extractor to use. A default <see cref="AspireTopologyExtractor"/> is created when
    /// <see langword="null"/>.
    /// </param>
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

        builder.Eventing.Subscribe<BeforeStartEvent>(async (startEvent, cancellationToken) =>
        {
            var logger = startEvent.Services
                .GetService<ILoggerFactory>()
                ?.CreateLogger(LoggerName);

            try
            {
                var topology = effectiveExtractor.Extract(startEvent.Model);
                var written = await writer.WriteAsync(topology, appHostDirectory, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var path in written)
                {
                    logger?.LogInformation("Wrote {Path}", path);
                }
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                // A diagram is not worth failing an app run over. The pipeline step still reports
                // the same failure loudly when someone asks for artifacts on purpose.
                logger?.LogWarning(exception, "Could not write topology artifacts.");
            }
        });
    }
}
