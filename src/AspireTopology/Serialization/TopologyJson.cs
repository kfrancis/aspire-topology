using System.Text.Json;
using System.Text.Json.Serialization;
using AspireTopology.Layout;
using AspireTopology.Model;

namespace AspireTopology.Serialization;

/// <summary>
/// Reads and writes the canonical <c>topology.json</c> representation of a topology.
/// </summary>
public static class TopologyJson
{
    /// <summary>
    /// The serializer options used for every AspireTopology artifact: camel-cased names, enums as
    /// strings, nulls omitted, and indentation so the output diffs well in source control.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            NewLine = "\n",
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    /// <summary>Serializes a topology document to JSON.</summary>
    /// <param name="document">The document to serialize.</param>
    /// <returns>The JSON text.</returns>
    public static string Serialize(TopologyDocument document) =>
        JsonSerializer.Serialize(document, Options);

    /// <summary>Deserializes a topology document from JSON.</summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The document.</returns>
    /// <exception cref="JsonException">The JSON did not describe a topology document.</exception>
    public static TopologyDocument Deserialize(string json) =>
        JsonSerializer.Deserialize<TopologyDocument>(json, Options)
        ?? throw new JsonException("The JSON did not contain a topology document.");

    /// <summary>Serializes a layout to JSON.</summary>
    /// <param name="layout">The layout to serialize.</param>
    /// <returns>The JSON text.</returns>
    public static string SerializeLayout(TopologyLayout layout) =>
        JsonSerializer.Serialize(layout, Options);

    /// <summary>Deserializes a layout from JSON.</summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The layout.</returns>
    /// <exception cref="JsonException">The JSON did not describe a layout.</exception>
    public static TopologyLayout DeserializeLayout(string json) =>
        JsonSerializer.Deserialize<TopologyLayout>(json, Options)
        ?? throw new JsonException("The JSON did not contain a topology layout.");
}
