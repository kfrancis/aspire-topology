using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting.ApplicationModel;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Classification;

/// <summary>Classifies Aspire project resources as services.</summary>
public sealed class ProjectResourceClassifier : ITopologyResourceClassifier
{
    /// <inheritdoc />
    public bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor)
    {
        descriptor = resource is ProjectResource ? new TopologyNodeDescriptor(TopologyNodeKind.Service) : null;
        return descriptor is not null;
    }
}

/// <summary>Classifies executables that were not recognised as something more specific.</summary>
public sealed class ExecutableResourceClassifier : ITopologyResourceClassifier
{
    /// <inheritdoc />
    public bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor)
    {
        descriptor = resource is ExecutableResource ? new TopologyNodeDescriptor(TopologyNodeKind.Executable) : null;
        return descriptor is not null;
    }
}

/// <summary>Classifies parameters.</summary>
public sealed class ParameterResourceClassifier : ITopologyResourceClassifier
{
    /// <inheritdoc />
    public bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor)
    {
        descriptor = resource is ParameterResource ? new TopologyNodeDescriptor(TopologyNodeKind.Parameter) : null;
        return descriptor is not null;
    }
}

/// <summary>Classifies containers that were not recognised as something more specific.</summary>
public sealed class ContainerResourceClassifier : ITopologyResourceClassifier
{
    /// <inheritdoc />
    public bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var isContainer = resource is ContainerResource
            || resource.Annotations.OfType<ContainerImageAnnotation>().Any();

        descriptor = isContainer ? new TopologyNodeDescriptor(TopologyNodeKind.Container) : null;
        return descriptor is not null;
    }
}

/// <summary>
/// Classifies anything no other classifier recognised.
/// </summary>
/// <remarks>
/// This classifier always succeeds. A resource AspireTopology has never heard of still appears on
/// the diagram, as <see cref="TopologyNodeKind.Unknown"/> and with its Aspire type recorded in the
/// node properties. Silently dropping resources would make the diagram quietly wrong.
/// </remarks>
public sealed class FallbackResourceClassifier : ITopologyResourceClassifier
{
    /// <inheritdoc />
    public bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor)
    {
        descriptor = new TopologyNodeDescriptor(TopologyNodeKind.Unknown);
        return true;
    }
}
