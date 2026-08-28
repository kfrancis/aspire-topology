using AspireTopology.Layout;
using AspireTopology.Model;

namespace AspireTopology.Isoflow.Layout;

/// <summary>
/// Projects abstract layout positions onto Isoflow's integer isometric grid.
/// </summary>
public sealed class IsoflowGridProjection
{
    /// <summary>Grid columns between adjacent nodes in the same layer.</summary>
    public int ColumnStep { get; init; } = 3;

    /// <summary>Grid rows between adjacent layers.</summary>
    public int RowStep { get; init; } = 3;

    /// <summary>Projects a layout onto the grid.</summary>
    /// <param name="topology">The topology the layout belongs to.</param>
    /// <param name="layout">The abstract layout.</param>
    /// <returns>A grid position per node identifier.</returns>
    public IReadOnlyDictionary<string, (int X, int Y)> Project(TopologyDocument topology, TopologyLayout layout)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(layout);

        var tiles = new Dictionary<string, (int X, int Y)>(StringComparer.Ordinal);
        var taken = new HashSet<(int X, int Y)>();

        foreach (var node in topology.Nodes)
        {
            var position = layout.Find(node.Id) ?? new TopologyPosition(0, 0);
            var tile = (
                X: (int)Math.Round(position.X * ColumnStep, MidpointRounding.AwayFromZero),
                Y: (int)Math.Round(position.Y * RowStep, MidpointRounding.AwayFromZero));

            // Two nodes must never share a tile; nudge sideways until the slot is free.
            while (!taken.Add(tile))
            {
                tile = (tile.X + 1, tile.Y);
            }

            tiles[node.Id] = tile;
        }

        return tiles;
    }
}
