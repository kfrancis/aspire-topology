using System.Globalization;
using AspireTopology.Isoflow.Model;
using AspireTopology.Model;

namespace AspireTopology.Isoflow.Mapping;

/// <summary>
/// Maps a <see cref="TopologyDocument"/> onto an <see cref="IsoflowDocument"/>.
/// </summary>
public sealed class TopologyToIsoflowMapper
{
    private readonly IsoflowRendererOptions _options;

    /// <summary>Creates a mapper.</summary>
    /// <param name="options">Rendering options. Defaults are used when <see langword="null"/>.</param>
    public TopologyToIsoflowMapper(IsoflowRendererOptions? options = null) =>
        _options = options ?? new IsoflowRendererOptions();

    /// <summary>Colour used for reference edges and group backgrounds.</summary>
    public const string ReferenceColorId = "reference";

    /// <summary>Colour used for startup dependency edges.</summary>
    public const string DependencyColorId = "dependency";

    /// <summary>Colour used for containment edges.</summary>
    public const string ParentColorId = "parent";

    /// <summary>Maps a topology to an Isoflow document.</summary>
    /// <param name="topology">The topology to map.</param>
    /// <returns>The Isoflow document.</returns>
    public IsoflowDocument Map(TopologyDocument topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        var layout = _options.LayoutEngine.Layout(topology);
        var tiles = _options.Projection.Project(topology, layout);

        var icons = topology.Nodes
            .Select(node => node.Kind)
            .Distinct()
            .OrderBy(kind => kind)
            .Select(TopologyIcons.Create)
            .ToList();

        var items = topology.Nodes
            .Select(node => new IsoflowItem(
                node.Id,
                node.Name,
                DescribeNode(node),
                TopologyIcons.IdFor(node.Kind)))
            .ToList();

        var viewItems = topology.Nodes
            .Select(node => new IsoflowViewItem(node.Id, new IsoflowTile(tiles[node.Id].X, tiles[node.Id].Y)))
            .ToList();

        var placed = viewItems.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);

        var connectors = topology.Edges
            .Where(edge => placed.Contains(edge.SourceId) && placed.Contains(edge.TargetId))
            .Select(MapConnector)
            .ToList();

        var rectangles = _options.RenderGroupRectangles
            ? MapGroupRectangles(topology, tiles)
            : [];

        var view = new IsoflowView(
            "view-default",
            _options.ViewName,
            viewItems,
            connectors,
            rectangles,
            []);

        return new IsoflowDocument(
            _options.Version,
            topology.Name,
            icons,
            Palette,
            items,
            [view]);
    }

    private static IReadOnlyList<IsoflowColor> Palette { get; } =
    [
        new IsoflowColor(ReferenceColorId, "#2563eb"),
        new IsoflowColor(DependencyColorId, "#94a3b8"),
        new IsoflowColor(ParentColorId, "#7c3aed"),
    ];

    private static IsoflowConnector MapConnector(TopologyEdge edge) =>
        new(
            edge.Id,
            [
                new IsoflowAnchor($"{edge.Id}-from", new IsoflowAnchorRef(Item: edge.SourceId)),
                new IsoflowAnchor($"{edge.Id}-to", new IsoflowAnchorRef(Item: edge.TargetId)),
            ],
            Description: edge.Kind.ToString(),
            Color: ColorFor(edge.Kind),
            Style: edge.Kind is TopologyEdgeKind.Dependency ? "DASHED" : "SOLID");

    private static string ColorFor(TopologyEdgeKind kind) => kind switch
    {
        TopologyEdgeKind.Dependency => DependencyColorId,
        TopologyEdgeKind.Parent => ParentColorId,
        _ => ReferenceColorId,
    };

    private static List<IsoflowRectangle> MapGroupRectangles(
        TopologyDocument topology,
        IReadOnlyDictionary<string, (int X, int Y)> tiles)
    {
        var rectangles = new List<IsoflowRectangle>();

        foreach (var group in topology.Groups)
        {
            var members = group.NodeIds
                .Where(tiles.ContainsKey)
                .Select(id => tiles[id])
                .ToList();

            if (members.Count == 0)
            {
                continue;
            }

            rectangles.Add(new IsoflowRectangle(
                $"rect-{group.Id}",
                new IsoflowTile(members.Min(t => t.X) - 1, members.Min(t => t.Y) - 1),
                new IsoflowTile(members.Max(t => t.X) + 1, members.Max(t => t.Y) + 1),
                ReferenceColorId));
        }

        return rectangles;
    }

    private static string? DescribeNode(TopologyNode node)
    {
        if (node.Properties.Count == 0)
        {
            return null;
        }

        var lines = node.Properties
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => $"{pair.Key}: {Format(pair.Value)}");

        return string.Join('\n', lines);
    }

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        bool flag => flag ? "true" : "false",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}
