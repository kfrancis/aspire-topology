using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Annotations;
using AspireTopology.Hosting.Classification;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// Turns a single Aspire resource into a topology node.
/// </summary>
public sealed class ResourceExtractor
{
    private readonly ResourceClassifier _classifier;
    private readonly EndpointExtractor _endpoints;
    private readonly TopologyDiagramOptions _options;

    /// <summary>Creates an extractor.</summary>
    /// <param name="options">Extraction options.</param>
    /// <param name="classifier">Decides what each resource is. Defaults are used when <see langword="null"/>.</param>
    public ResourceExtractor(TopologyDiagramOptions options, ResourceClassifier? classifier = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        _classifier = classifier ?? new ResourceClassifier();
        _endpoints = new EndpointExtractor();
    }

    /// <summary>Decides whether a resource belongs in the topology at all.</summary>
    /// <param name="resource">The resource to test.</param>
    /// <returns><see langword="true"/> when the resource should appear.</returns>
    public bool ShouldInclude(IResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (Metadata(resource)?.Exclude == true)
        {
            return false;
        }

        // Resources Aspire always hides from its own dashboard are orchestration plumbing, not
        // architecture. Run mode adds several of them, such as the project rebuilders behind hot
        // reload, and they would otherwise appear only in diagrams generated on start.
        // HiddenBehavior.OnCompletion is different: a migration job that disappears once it
        // succeeds is still part of the architecture, so it stays.
        if (resource.Annotations.OfType<HiddenAnnotation>().Any(hidden => hidden.Behavior is HiddenBehavior.Always))
        {
            return false;
        }

        return _options.IncludeParameters || resource is not ParameterResource;
    }

    /// <summary>Extracts a node from a resource.</summary>
    /// <param name="resource">The resource to extract.</param>
    /// <returns>The node.</returns>
    public TopologyNode Extract(IResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var metadata = Metadata(resource);
        var descriptor = _classifier.Classify(resource);
        var kind = metadata?.Kind ?? descriptor.Kind;

        var properties = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [TopologyPropertyNames.AspireName] = resource.Name,
            [TopologyPropertyNames.AspireResourceType] = resource.GetType().Name,
        };

        if (resource is IResourceWithParent withParent)
        {
            properties[TopologyPropertyNames.AspireParent] = withParent.Parent.Name;
        }

        AddContainerProperties(resource, properties);

        if (_options.IncludeEndpoints)
        {
            _endpoints.Extract(resource, properties);
        }

        if (metadata?.Group is { Length: > 0 } group)
        {
            properties[TopologyPropertyNames.Group] = group;
        }

        if (metadata?.Description is { Length: > 0 } description)
        {
            properties[TopologyPropertyNames.Description] = description;
        }

        if ((metadata?.Icon ?? descriptor.Icon) is { Length: > 0 } icon)
        {
            properties[TopologyPropertyNames.Icon] = icon;
        }

        return new TopologyNode(
            resource.Name,
            metadata?.DisplayName ?? resource.Name,
            kind,
            properties);
    }

    private static void AddContainerProperties(IResource resource, IDictionary<string, object?> properties)
    {
        if (resource.Annotations.OfType<ContainerImageAnnotation>().LastOrDefault() is not { } image)
        {
            return;
        }

        properties[TopologyPropertyNames.ContainerImage] = image.Image;

        if (image.Tag is { Length: > 0 } tag)
        {
            properties[TopologyPropertyNames.ContainerTag] = tag;
        }

        if (image.Registry is { Length: > 0 } registry)
        {
            properties[TopologyPropertyNames.ContainerRegistry] = registry;
        }
    }

    private static TopologyMetadataAnnotation? Metadata(IResource resource) =>
        resource.Annotations.OfType<TopologyMetadataAnnotation>().LastOrDefault();
}
