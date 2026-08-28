using Aspire.Hosting.ApplicationModel;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Annotations;

/// <summary>
/// Diagram metadata attached to an Aspire resource.
/// </summary>
/// <remarks>
/// Annotations travel with the resource, so an application or a third-party integration can
/// describe how it should appear without AspireTopology needing to know about it, and without a
/// side file that maps resource names to display names.
/// </remarks>
public sealed class TopologyMetadataAnnotation : IResourceAnnotation
{
    /// <summary>Name shown on the diagram instead of the resource name.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Free text describing the resource.</summary>
    public string? Description { get; set; }

    /// <summary>Name of a logical group the resource belongs to, for example "Backend".</summary>
    public string? Group { get; set; }

    /// <summary>Renderer-specific icon hint.</summary>
    public string? Icon { get; set; }

    /// <summary>Overrides the classification the extractor would otherwise infer.</summary>
    public TopologyNodeKind? Kind { get; set; }

    /// <summary>When <see langword="true"/>, the resource is left out of the topology entirely.</summary>
    public bool Exclude { get; set; }
}
