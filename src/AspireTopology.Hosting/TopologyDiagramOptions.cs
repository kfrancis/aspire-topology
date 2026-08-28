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
    /// On by default, and only ever applies in run mode. A failure to write is logged as a
    /// warning rather than failing the run.
    /// </remarks>
    public bool GenerateOnStart { get; set; } = true;

    /// <summary>
    /// Serve the topology viewer from inside the AppHost and list it in the Aspire dashboard, the
    /// way an integration lists a management UI.
    /// </summary>
    /// <remarks>
    /// On by default, and only ever applies in run mode. The viewer needs no container runtime and
    /// no Node.js: its front end is embedded in this package, and it renders the topology from the
    /// live application model on each request rather than from a file.
    /// </remarks>
    public bool Viewer { get; set; } = true;

    /// <summary>Name of the viewer resource shown in the dashboard.</summary>
    public string ViewerResourceName { get; set; } = "topology";

    /// <summary>
    /// The renderers run after <c>topology.json</c> is written. Defaults to Isoflow.
    /// </summary>
    public IList<ITopologyRenderer> Renderers { get; } = [new IsoflowTopologyRenderer()];
}
