namespace AspireTopology.Model;

/// <summary>
/// A single participant in the topology.
/// </summary>
/// <param name="Id">Stable identifier, unique within a <see cref="TopologyDocument"/>.</param>
/// <param name="Name">Display name.</param>
/// <param name="Kind">Semantic classification.</param>
/// <param name="Properties">
/// Additional metadata carried through from the source model. Never contains secrets, parameter
/// values, environment values or connection strings.
/// </param>
[method: System.Text.Json.Serialization.JsonConstructor]
public sealed record TopologyNode(
    string Id,
    string Name,
    TopologyNodeKind Kind,
    IReadOnlyDictionary<string, object?> Properties)
{
    /// <summary>Creates a node with no properties.</summary>
    public TopologyNode(string id, string name, TopologyNodeKind kind)
        : this(id, name, kind, EmptyProperties)
    {
    }

    /// <summary>An empty, shared property bag.</summary>
    public static IReadOnlyDictionary<string, object?> EmptyProperties { get; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);
}
