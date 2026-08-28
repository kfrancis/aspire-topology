namespace AspireTopology.Layout;

/// <summary>
/// A position in an abstract, renderer-independent coordinate space.
/// </summary>
/// <param name="X">Horizontal coordinate. Increases to the right.</param>
/// <param name="Y">Vertical coordinate. Increases downwards, away from the roots of the graph.</param>
public sealed record TopologyPosition(double X, double Y);
