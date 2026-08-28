using AspireTopology.Model;

namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// A relationship discovered in the Aspire application model, before it is turned into an edge.
/// </summary>
/// <param name="SourceId">Name of the resource the relationship originates from.</param>
/// <param name="TargetId">Name of the resource the relationship points at.</param>
/// <param name="Kind">The nature of the relationship.</param>
public sealed record TopologyRelationship(
    string SourceId,
    string TargetId,
    TopologyEdgeKind Kind);
