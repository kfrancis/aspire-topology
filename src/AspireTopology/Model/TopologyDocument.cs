namespace AspireTopology.Model;

/// <summary>
/// A renderer-independent description of what exists in an application and how it is related.
/// </summary>
/// <remarks>
/// A document answers "what exists and how is it related". It deliberately carries no coordinates;
/// where things appear is the job of a <c>TopologyLayout</c>.
/// </remarks>
/// <param name="Name">Name of the application the topology describes.</param>
/// <param name="Nodes">The participants.</param>
/// <param name="Edges">The relationships between participants.</param>
/// <param name="Groups">Named sets of nodes.</param>
[method: System.Text.Json.Serialization.JsonConstructor]
public sealed record TopologyDocument(
    string Name,
    IReadOnlyList<TopologyNode> Nodes,
    IReadOnlyList<TopologyEdge> Edges,
    IReadOnlyList<TopologyGroup> Groups)
{
    /// <summary>Creates a document with no groups.</summary>
    public TopologyDocument(string name, IReadOnlyList<TopologyNode> nodes, IReadOnlyList<TopologyEdge> edges)
        : this(name, nodes, edges, [])
    {
    }

    /// <summary>Creates an empty document.</summary>
    /// <param name="name">Name of the application.</param>
    public static TopologyDocument Empty(string name) => new(name, [], [], []);

    /// <summary>Finds a node by identifier.</summary>
    /// <param name="id">The node identifier.</param>
    /// <returns>The node, or <see langword="null"/> when no node has that identifier.</returns>
    public TopologyNode? FindNode(string id) =>
        Nodes.FirstOrDefault(n => string.Equals(n.Id, id, StringComparison.Ordinal));
}
