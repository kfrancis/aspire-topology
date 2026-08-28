namespace AspireTopology.Model;

/// <summary>
/// A named set of nodes that belong together.
/// </summary>
/// <param name="Id">Stable identifier, unique within a <see cref="TopologyDocument"/>.</param>
/// <param name="Name">Display name.</param>
/// <param name="Kind">Why the nodes are grouped.</param>
/// <param name="NodeIds">Identifiers of the member nodes.</param>
public sealed record TopologyGroup(
    string Id,
    string Name,
    TopologyGroupKind Kind,
    IReadOnlyList<string> NodeIds);
