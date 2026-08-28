using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Annotations;

namespace AspireTopology.Hosting.Viewer;

/// <summary>
/// The topology viewer, as it appears in the Aspire dashboard.
/// </summary>
/// <remarks>
/// The viewer has no process and no container: it is served from inside the AppHost, so this
/// resource has no lifetime of its own. Its state and URL are published by
/// <see cref="TopologyViewerService"/>.
/// </remarks>
public sealed class TopologyViewerResource : Resource, IResourceWithoutLifetime
{
    /// <summary>The resource type shown in the dashboard.</summary>
    public const string ResourceTypeName = "TopologyViewer";

    /// <summary>Creates the viewer resource.</summary>
    /// <param name="name">The resource name.</param>
    public TopologyViewerResource(string name)
        : base(name)
    {
        // The viewer is tooling, not architecture. It must never appear in its own diagram.
        Annotations.Add(new TopologyMetadataAnnotation { Exclude = true });
        Annotations.Add(new ResourceIconAnnotation("Map", IconVariant.Filled));
    }
}
