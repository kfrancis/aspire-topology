using AspireTopology.Isoflow;
using AspireTopology.Rendering;

namespace AspireTopology.Hosting;

/// <summary>
/// Controls what AspireTopology extracts and where it writes the result.
/// </summary>
public sealed class TopologyDiagramOptions
{
    /// <summary>
    /// Directory the artifacts are written to, relative to the AppHost directory unless it is an
    /// absolute path.
    /// </summary>
    public string OutputPath { get; set; } = "artifacts/topology";

    /// <summary>Base file name of the generated artifacts, without an extension.</summary>
    public string FileName { get; set; } = "topology";

    /// <summary>
    /// Name written into the topology document. When <see langword="null"/>, the AppHost
    /// application name is used.
    /// </summary>
    public string? DocumentName { get; set; }

    /// <summary>Whether parameter resources appear in the topology. Off by default: parameters are configuration, not architecture.</summary>
    public bool IncludeParameters { get; set; }

    /// <summary>Whether declared endpoints are copied into node properties.</summary>
    public bool IncludeEndpoints { get; set; } = true;

    /// <summary>
    /// Also regenerate the artifacts every time the AppHost starts, so running the app keeps the
    /// diagram current without anyone remembering <c>aspire do topology</c>.
    /// </summary>
    /// <remarks>
    /// Off by default, and only ever applies in run mode. A failure to write is logged as a
    /// warning rather than failing the run.
    /// </remarks>
    public bool GenerateOnStart { get; set; }

    /// <summary>
    /// The renderers run after <c>topology.json</c> is written. Defaults to Isoflow.
    /// </summary>
    public IList<ITopologyRenderer> Renderers { get; } = [new IsoflowTopologyRenderer()];
}
