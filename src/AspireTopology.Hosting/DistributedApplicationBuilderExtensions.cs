using AspireTopology.Hosting;
using AspireTopology.Hosting.Pipeline;
using AspireTopology.Hosting.Startup;
using AspireTopology.Hosting.Viewer;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting;

/// <summary>
/// Adds topology generation to an Aspire AppHost.
/// </summary>
public static class DistributedApplicationBuilderExtensions
{
    /// <summary>
    /// Registers the <c>topology</c> pipeline step, which reads the application model and writes
    /// a topology document plus one file per configured renderer.
    /// </summary>
    /// <param name="builder">The AppHost builder.</param>
    /// <param name="configure">Configures the options.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.AddTopologyDiagram();
    /// </code>
    /// Then run <c>aspire do topology</c>.
    /// </example>
    public static IDistributedApplicationBuilder AddTopologyDiagram(
        this IDistributedApplicationBuilder builder,
        Action<TopologyDiagramOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new TopologyDiagramOptions();
        configure?.Invoke(options);
        options.DocumentName ??= builder.Environment.ApplicationName;

        TopologyPipelineStep.Add(builder, options);

        // Only in run mode: publish and pipeline runs already go through the step above, and
        // subscribing there would write the same files twice.
        if (options.GenerateOnStart && builder.ExecutionContext.IsRunMode)
        {
            TopologyStartupGeneration.Add(builder, options);
        }

        // The viewer is served from inside the AppHost, so there is nothing to publish or deploy.
        if (options.Viewer && builder.ExecutionContext.IsRunMode)
        {
            AddViewer(builder, options);
        }

        return builder;
    }

    private static void AddViewer(IDistributedApplicationBuilder builder, TopologyDiagramOptions options)
    {
        var resource = new TopologyViewerResource(options.ViewerResourceName);
        builder.AddResource(resource);

        builder.Services.AddHostedService(services => new TopologyViewerService(
            resource,
            options,
            services.GetRequiredService<DistributedApplicationModel>(),
            services.GetRequiredService<ResourceNotificationService>(),
            services.GetRequiredService<ILogger<TopologyViewerService>>()));
    }
}
