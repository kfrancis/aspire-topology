namespace AspireTopology.Layout;

/// <summary>
/// Where the nodes of a topology should appear.
/// </summary>
/// <remarks>
/// Layout is kept out of <c>TopologyDocument</c> on purpose. The same topology is arranged
/// differently by an isometric grid, an automatic graph layout and a hand-edited diagram, and a
/// human-owned layout file should survive regeneration of the topology.
/// </remarks>
/// <param name="Nodes">Position of each node, keyed by node identifier.</param>
public sealed record TopologyLayout(IReadOnlyDictionary<string, TopologyPosition> Nodes)
{
    /// <summary>An empty layout.</summary>
    public static TopologyLayout Empty { get; } =
        new(new Dictionary<string, TopologyPosition>(StringComparer.Ordinal));

    /// <summary>Gets the position of a node.</summary>
    /// <param name="nodeId">The node identifier.</param>
    /// <returns>The position, or <see langword="null"/> when the node was not laid out.</returns>
    public TopologyPosition? Find(string nodeId) =>
        Nodes.TryGetValue(nodeId, out var position) ? position : null;

    /// <summary>
    /// Returns a layout in which positions from <paramref name="overrides"/> replace the
    /// positions in this layout.
    /// </summary>
    /// <param name="overrides">Positions that take precedence, typically human-owned.</param>
    /// <returns>The merged layout.</returns>
    public TopologyLayout WithOverrides(TopologyLayout overrides)
    {
        ArgumentNullException.ThrowIfNull(overrides);

        var merged = new Dictionary<string, TopologyPosition>(Nodes, StringComparer.Ordinal);
        foreach (var (id, position) in overrides.Nodes)
        {
            merged[id] = position;
        }

        return new TopologyLayout(merged);
    }
}
