namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// The property keys AspireTopology writes onto nodes.
/// </summary>
/// <remarks>
/// Property names are namespaced so a renderer can tell where a value came from, and so future
/// sources of metadata do not collide with the ones written here.
/// </remarks>
public static class TopologyPropertyNames
{
    /// <summary>The CLR type name of the Aspire resource, for example <c>PostgresResource</c>.</summary>
    public const string AspireResourceType = "aspire.resourceType";

    /// <summary>The Aspire resource name.</summary>
    public const string AspireName = "aspire.name";

    /// <summary>The name of the parent resource, when the resource is contained by another.</summary>
    public const string AspireParent = "aspire.parent";

    /// <summary>The container image, without registry or tag.</summary>
    public const string ContainerImage = "container.image";

    /// <summary>The container image tag.</summary>
    public const string ContainerTag = "container.tag";

    /// <summary>The container registry.</summary>
    public const string ContainerRegistry = "container.registry";

    /// <summary>The logical group the resource was assigned by an annotation.</summary>
    public const string Group = "topology.group";

    /// <summary>A free-text description supplied by an annotation.</summary>
    public const string Description = "topology.description";

    /// <summary>A renderer-specific icon hint supplied by an annotation or a classifier.</summary>
    public const string Icon = "topology.icon";

    /// <summary>Builds the property key for one field of one endpoint.</summary>
    /// <param name="endpointName">The endpoint name.</param>
    /// <param name="field">The field, for example <c>port</c>.</param>
    /// <returns>The property key.</returns>
    public static string Endpoint(string endpointName, string field) =>
        $"endpoint.{endpointName}.{field}";
}
