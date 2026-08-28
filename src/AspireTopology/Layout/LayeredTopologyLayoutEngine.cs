using AspireTopology.Model;

namespace AspireTopology.Layout;

/// <summary>
/// Arranges nodes in horizontal layers by dependency depth: things nothing points at sit on the
/// top layer, the things they point at sit below them, and so on.
/// </summary>
/// <remarks>
/// This is deliberately simple. It is deterministic, it terminates on cyclic graphs, and it is
/// good enough to read a small application at a glance. Replacing it with a real graph layout
/// engine later is a matter of supplying a different <see cref="ITopologyLayoutEngine"/>.
/// </remarks>
public sealed class LayeredTopologyLayoutEngine : ITopologyLayoutEngine
{
    /// <summary>Horizontal distance between two nodes in the same layer.</summary>
    public double ColumnSpacing { get; init; } = 1;

    /// <summary>Vertical distance between two layers.</summary>
    public double RowSpacing { get; init; } = 1;

    /// <summary>
    /// Edge kinds that contribute to depth. Containment is excluded so that a database does not
    /// get pushed a layer below its own server.
    /// </summary>
    private static bool IsDepthEdge(TopologyEdge edge) => edge.Kind is TopologyEdgeKind.Reference or TopologyEdgeKind.Dependency;

    /// <inheritdoc />
    public TopologyLayout Layout(TopologyDocument topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        var depths = ComputeDepths(topology);
        var positions = new Dictionary<string, TopologyPosition>(StringComparer.Ordinal);

        var layers = depths
            .GroupBy(pair => pair.Value)
            .OrderBy(group => group.Key);

        foreach (var layer in layers)
        {
            var ordered = layer
                .Select(pair => pair.Key)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            // Centre each layer around x = 0 so wide layers grow outwards symmetrically.
            var offset = (ordered.Count - 1) / 2.0;

            for (var index = 0; index < ordered.Count; index++)
            {
                positions[ordered[index]] = new TopologyPosition(
                    (index - offset) * ColumnSpacing,
                    layer.Key * RowSpacing);
            }
        }

        return new TopologyLayout(positions);
    }

    private static Dictionary<string, int> ComputeDepths(TopologyDocument topology)
    {
        var depths = topology.Nodes.ToDictionary(node => node.Id, _ => 0, StringComparer.Ordinal);

        var outgoing = topology.Edges
            .Where(IsDepthEdge)
            .Where(edge => depths.ContainsKey(edge.SourceId) && depths.ContainsKey(edge.TargetId))
            .GroupBy(edge => edge.SourceId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(edge => edge.TargetId).Distinct(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);

        // Relax depths until they stop changing. Bounded by the node count so cycles terminate.
        var maxPasses = topology.Nodes.Count;
        for (var pass = 0; pass < maxPasses; pass++)
        {
            var changed = false;

            foreach (var (sourceId, targetIds) in outgoing)
            {
                var candidate = depths[sourceId] + 1;
                foreach (var targetId in targetIds)
                {
                    if (depths[targetId] < candidate && candidate <= maxPasses)
                    {
                        depths[targetId] = candidate;
                        changed = true;
                    }
                }
            }

            if (!changed)
            {
                break;
            }
        }

        return depths;
    }
}
