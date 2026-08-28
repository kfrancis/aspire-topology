namespace AspireTopology.Model;

/// <summary>
/// The reason a set of nodes is grouped together.
/// </summary>
public enum TopologyGroupKind
{
    /// <summary>The grouping has no defined meaning.</summary>
    Unknown = 0,

    /// <summary>The nodes are contained by a single parent node.</summary>
    Containment,

    /// <summary>The nodes were grouped by the author, for example into "Backend".</summary>
    Logical,
}
