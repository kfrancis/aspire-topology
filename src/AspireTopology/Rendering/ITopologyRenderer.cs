using AspireTopology.Model;

namespace AspireTopology.Rendering;

/// <summary>
/// Turns a topology document into a concrete diagram format.
/// </summary>
public interface ITopologyRenderer
{
    /// <summary>
    /// Short, file-name-safe name of the format, for example <c>isoflow</c>. It becomes part of
    /// the generated file name.
    /// </summary>
    string Name { get; }

    /// <summary>File extension of the rendered output, including the leading dot.</summary>
    string FileExtension { get; }

    /// <summary>Writes <paramref name="topology"/> to <paramref name="output"/>.</summary>
    /// <param name="topology">The topology to render.</param>
    /// <param name="output">The stream the rendered diagram is written to.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>A task that completes when the diagram has been written.</returns>
    ValueTask RenderAsync(
        TopologyDocument topology,
        Stream output,
        CancellationToken cancellationToken = default);
}
