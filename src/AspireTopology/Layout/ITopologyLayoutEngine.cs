using AspireTopology.Model;

namespace AspireTopology.Layout;

/// <summary>
/// Decides where the nodes of a topology should appear.
/// </summary>
public interface ITopologyLayoutEngine
{
    /// <summary>Computes positions for every node in <paramref name="topology"/>.</summary>
    /// <param name="topology">The topology to lay out.</param>
    /// <returns>The computed layout.</returns>
    TopologyLayout Layout(TopologyDocument topology);
}
