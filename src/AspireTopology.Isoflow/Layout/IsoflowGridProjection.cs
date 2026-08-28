using AspireTopology.Layout;
using AspireTopology.Model;

namespace AspireTopology.Isoflow.Layout;

/// <summary>
/// Projects abstract layout positions onto Isoflow's integer isometric grid.
/// </summary>
public sealed class IsoflowGridProjection
{
    /// <summary>Grid columns between adjacent nodes in the same layer.</summary>
    public int ColumnStep { get; init; } = 4;

    /// <summary>Grid rows between adjacent layers.</summary>
    public int RowStep { get; init; } = 4;

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

        return Centre(tiles);
    }

    /// <summary>
    /// Shifts every tile so the diagram straddles the grid origin.
    /// </summary>
    /// <remarks>
    /// Isoflow opens looking at tile (0, 0) and does not fit the content to the viewport, so a
    /// diagram that grows downwards from the origin opens with half of itself off screen.
    /// </remarks>
    private static IReadOnlyDictionary<string, (int X, int Y)> Centre(Dictionary<string, (int X, int Y)> tiles)
    {
        if (tiles.Count == 0)
        {
            return tiles;
        }

        var offsetX = (tiles.Values.Min(tile => tile.X) + tiles.Values.Max(tile => tile.X)) / 2;
        var offsetY = (tiles.Values.Min(tile => tile.Y) + tiles.Values.Max(tile => tile.Y)) / 2;

        // Centred exactly, which also puts a node on tile (0, 0). Isoflow draws a cursor highlight
        // there, and an occupied origin hides it; an offset diagram leaves it floating in space.
        return tiles.ToDictionary(
            entry => entry.Key,
            entry => (entry.Value.X - offsetX, entry.Value.Y - offsetY),
            StringComparer.Ordinal);
    }
}
