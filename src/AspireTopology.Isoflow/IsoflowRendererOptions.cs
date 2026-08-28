using AspireTopology.Isoflow.Layout;
using AspireTopology.Layout;

namespace AspireTopology.Isoflow;

/// <summary>
/// Controls how a topology is turned into an Isoflow document.
/// </summary>
public sealed class IsoflowRendererOptions
{
    /// <summary>Schema version written into the document.</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Name of the generated view.</summary>
    public string ViewName { get; set; } = "Architecture";

    /// <summary>Decides where nodes sit before they are projected onto the grid.</summary>
    public ITopologyLayoutEngine LayoutEngine { get; set; } = new LayeredTopologyLayoutEngine();

    /// <summary>Projects layout positions onto the isometric grid.</summary>
    public IsoflowGridProjection Projection { get; set; } = new();

    /// <summary>Whether containment groups are drawn as background rectangles.</summary>
    public bool RenderGroupRectangles { get; set; } = true;
}
