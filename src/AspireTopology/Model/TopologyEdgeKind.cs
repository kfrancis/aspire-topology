namespace AspireTopology.Model;

/// <summary>
/// The nature of the relationship an edge represents.
/// </summary>
/// <remarks>
/// Edge kinds describe what the source model declared, not what the application does at runtime.
/// A <see cref="Reference"/> means one resource was given the other's connection information; it
/// does not assert that a call is ever made.
/// </remarks>
public enum TopologyEdgeKind
{
    /// <summary>The source was given connection information for the target.</summary>
    Reference = 0,

    /// <summary>The source waits for the target before starting.</summary>
    Dependency,

    /// <summary>The target contains the source, such as a database inside a database server.</summary>
    Parent,

    /// <summary>The relationship was observed at runtime rather than declared.</summary>
    Observed,
}
