namespace AspireTopology.Model;

/// <summary>
/// A directed relationship between two nodes.
/// </summary>
/// <param name="Id">Stable identifier, unique within a <see cref="TopologyDocument"/>.</param>
/// <param name="SourceId">Identifier of the node the relationship originates from.</param>
/// <param name="TargetId">Identifier of the node the relationship points at.</param>
/// <param name="Kind">The nature of the relationship.</param>
public sealed record TopologyEdge(
    string Id,
    string SourceId,
    string TargetId,
    TopologyEdgeKind Kind);
