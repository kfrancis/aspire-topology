using System.Text.Json;
using AspireTopology.Isoflow.Mapping;
using AspireTopology.Model;
using AspireTopology.Rendering;
using AspireTopology.Serialization;

namespace AspireTopology.Isoflow;

/// <summary>
/// Renders a topology as Isoflow <c>initialData</c> JSON.
/// </summary>
public sealed class IsoflowTopologyRenderer : ITopologyRenderer
{
    private readonly TopologyToIsoflowMapper _mapper;

    /// <summary>Creates a renderer.</summary>
    /// <param name="options">Rendering options. Defaults are used when <see langword="null"/>.</param>
    public IsoflowTopologyRenderer(IsoflowRendererOptions? options = null) =>
        _mapper = new TopologyToIsoflowMapper(options);

    /// <inheritdoc />
    public string Name => "isoflow";

    /// <inheritdoc />
    public string FileExtension => ".json";

    /// <summary>Maps a topology to an Isoflow document without serializing it.</summary>
    /// <param name="topology">The topology to map.</param>
    /// <returns>The Isoflow document.</returns>
    public Model.IsoflowDocument Map(TopologyDocument topology) => _mapper.Map(topology);

    /// <inheritdoc />
    public async ValueTask RenderAsync(
        TopologyDocument topology,
        Stream output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(output);

        var document = _mapper.Map(topology);
        await JsonSerializer.SerializeAsync(output, document, TopologyJson.Options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Renders a topology to a JSON string.</summary>
    /// <param name="topology">The topology to render.</param>
    /// <returns>The Isoflow JSON.</returns>
    public string RenderToString(TopologyDocument topology) =>
        JsonSerializer.Serialize(_mapper.Map(topology), TopologyJson.Options);
}
