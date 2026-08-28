using System.Text;
using AspireTopology.Model;
using AspireTopology.Serialization;

namespace AspireTopology.Hosting;

/// <summary>
/// Writes the topology document and every configured renderer output to disk.
/// </summary>
public sealed class TopologyArtifactWriter
{
    private readonly TopologyDiagramOptions _options;

    // No byte order mark: the output is JSON that other tools parse.
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    /// <summary>Creates a writer.</summary>
    /// <param name="options">Options describing where and what to write.</param>
    public TopologyArtifactWriter(TopologyDiagramOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>Writes the artifacts.</summary>
    /// <param name="topology">The topology to write.</param>
    /// <param name="baseDirectory">Directory that a relative <see cref="TopologyDiagramOptions.OutputPath"/> is resolved against.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The full paths of the files written, in the order they were written.</returns>
    public async Task<IReadOnlyList<string>> WriteAsync(
        TopologyDocument topology,
        string baseDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentException.ThrowIfNullOrEmpty(baseDirectory);

        var directory = Path.IsPathRooted(_options.OutputPath)
            ? _options.OutputPath
            : Path.Combine(baseDirectory, _options.OutputPath);

        Directory.CreateDirectory(directory);

        var written = new List<string>();

        var documentPath = Path.Combine(directory, $"{_options.FileName}.json");
        await File.WriteAllTextAsync(documentPath, TopologyJson.Serialize(topology), Utf8NoBom, cancellationToken)
            .ConfigureAwait(false);
        written.Add(documentPath);

        foreach (var renderer in _options.Renderers)
        {
            var path = Path.Combine(directory, $"{_options.FileName}.{renderer.Name}{renderer.FileExtension}");

            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            await using (stream.ConfigureAwait(false))
            {
                await renderer.RenderAsync(topology, stream, cancellationToken).ConfigureAwait(false);
            }

            written.Add(path);
        }

        return written;
    }
}
