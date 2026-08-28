using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Annotations;

namespace Aspire.Hosting;

/// <summary>
/// Adds diagram metadata to Aspire resources.
/// </summary>
public static class TopologyResourceBuilderExtensions
{
    /// <summary>
    /// Describes how a resource should appear in generated topology diagrams.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configure">Configures the metadata.</param>
    /// <returns>The resource builder, for chaining.</returns>
    /// <example>
    /// <code>
    /// api.WithTopologyMetadata(x =>
    /// {
    ///     x.DisplayName = "Public API";
    ///     x.Group = "Backend";
    /// });
    /// </code>
    /// </example>
    public static IResourceBuilder<T> WithTopologyMetadata<T>(
        this IResourceBuilder<T> builder,
        Action<TopologyMetadataAnnotation> configure)
        where T : IResource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var annotation = builder.Resource.Annotations.OfType<TopologyMetadataAnnotation>().LastOrDefault();
        if (annotation is null)
        {
            annotation = new TopologyMetadataAnnotation();
            builder.Resource.Annotations.Add(annotation);
        }

        configure(annotation);
        return builder;
    }

    /// <summary>Leaves a resource out of generated topology diagrams.</summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <returns>The resource builder, for chaining.</returns>
    public static IResourceBuilder<T> ExcludeFromTopology<T>(this IResourceBuilder<T> builder)
        where T : IResource =>
        builder.WithTopologyMetadata(x => x.Exclude = true);
}
