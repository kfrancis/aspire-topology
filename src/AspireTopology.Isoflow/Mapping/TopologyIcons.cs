using System.Globalization;
using System.Text;
using AspireTopology.Isoflow.Model;
using AspireTopology.Model;

namespace AspireTopology.Isoflow.Mapping;

/// <summary>
/// Supplies one icon per <see cref="TopologyNodeKind"/>, drawn in isometric projection and in
/// Aspire's palette.
/// </summary>
/// <remarks>
/// The icons are drawn already projected and flagged <c>isIsometric</c>, which is how Isoflow's own
/// icon packs work: the renderer places them on the grid as solid objects rather than projecting a
/// flat image, which turns a square into a hard diamond.
/// <para>
/// They are inline SVG data URIs, so a generated diagram renders on its own with no icon pack
/// installed and no network access. A viewer that has <c>@isoflow/isopacks</c> available can
/// override them by merging its own icon list ahead of these, keyed by the same identifiers.
/// </para>
/// </remarks>
public static class TopologyIcons
{
    /// <summary>Prefix shared by every built-in icon identifier.</summary>
    public const string IdPrefix = "aspire-topology-";

    /// <summary>Name of the collection the built-in icons belong to.</summary>
    public const string CollectionName = "AspireTopology";

    /// <summary>
    /// Shapes the icon set is built from. Keeping the vocabulary small is what makes a diagram
    /// readable at a glance: the silhouette says what family something belongs to, and the colour
    /// says which member of the family it is.
    /// </summary>
    private enum Shape
    {
        /// <summary>A solid box. Things that run.</summary>
        Cube,

        /// <summary>A drum. Things that store records.</summary>
        Cylinder,

        /// <summary>Stacked plates. Things that store bytes.</summary>
        Stack,

        /// <summary>A thin plate. Things that are values rather than processes.</summary>
        Plate,

        /// <summary>A ball. Things outside the application.</summary>
        Sphere,
    }

    // Compute-shaped things carry Aspire's brand purples. Data-shaped things borrow the hues Aspire
    // already uses to tell kinds apart in its API documentation, so the set reads as one family.
    private static readonly Dictionary<TopologyNodeKind, (Shape Shape, string Color)> Designs = new()
    {
        [TopologyNodeKind.Service] = (Shape.Cube, AspirePalette.Purple),
        [TopologyNodeKind.Container] = (Shape.Cube, AspirePalette.Primary),
        [TopologyNodeKind.Executable] = (Shape.Cube, AspirePalette.Black),
        [TopologyNodeKind.Database] = (Shape.Cylinder, AspirePalette.Green),
        [TopologyNodeKind.Cache] = (Shape.Cylinder, AspirePalette.Magenta),
        [TopologyNodeKind.MessageBroker] = (Shape.Stack, AspirePalette.Amber),
        [TopologyNodeKind.Storage] = (Shape.Stack, AspirePalette.Teal),
        [TopologyNodeKind.ExternalService] = (Shape.Sphere, AspirePalette.Blue),
        [TopologyNodeKind.Parameter] = (Shape.Plate, AspirePalette.Muted),
        [TopologyNodeKind.Unknown] = (Shape.Cube, AspirePalette.Muted),
    };

    /// <summary>Returns the icon identifier used for a node kind.</summary>
    /// <param name="kind">The node kind.</param>
    /// <returns>The icon identifier.</returns>
    public static string IdFor(TopologyNodeKind kind) =>
        IdPrefix + kind.ToString().ToLowerInvariant();

    /// <summary>Returns the base colour used for a node kind.</summary>
    /// <param name="kind">The node kind.</param>
    /// <returns>The CSS colour value.</returns>
    public static string ColorFor(TopologyNodeKind kind) =>
        Designs.TryGetValue(kind, out var design) ? design.Color : AspirePalette.Muted;

    /// <summary>Builds the icon definition for a node kind.</summary>
    /// <param name="kind">The node kind.</param>
    /// <returns>The icon.</returns>
    public static IsoflowIcon Create(TopologyNodeKind kind)
    {
        var (shape, color) = Designs.TryGetValue(kind, out var design) ? design : Designs[TopologyNodeKind.Unknown];
        var svg = Draw(shape, color);
        var url = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

        return new IsoflowIcon(IdFor(kind), Humanize(kind), url, CollectionName, IsIsometric: true);
    }

    // The isometric frame every shape is drawn in: a 2:1 rhombus centred on (50, 46), which is
    // where Isoflow's tile sits, with the object standing on it.
    private const string Open =
        """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="100" height="100">""";

    private const string Close = "</svg>";

    private static string Draw(Shape shape, string color)
    {
        var top = Mix(color, "#ffffff", 0.34);
        var left = Mix(color, "#000000", 0.10);
        var right = Mix(color, "#000000", 0.30);
        var edge = Mix(color, "#000000", 0.42);

        var body = shape switch
        {
            Shape.Cube => Cube(top, left, right, edge, height: 26),
            Shape.Cylinder => Cylinder(top, left, right, edge, height: 26),
            Shape.Stack => Stack(top, left, right, edge),
            Shape.Plate => Cube(top, left, right, edge, height: 8),
            Shape.Sphere => Sphere(top, left, right, edge),
            _ => Cube(top, left, right, edge, height: 26),
        };

        return Open + body + Close;
    }

    /// <summary>A box standing on the tile, drawn as its top, left and right faces.</summary>
    private static string Cube(string top, string left, string right, string edge, double height) =>
        CubeAt(top, left, right, edge, height, baseY: 62 - (height / 2));

    /// <summary>A drum standing on the tile.</summary>
    private static string Cylinder(string top, string left, string right, string edge, double height)
    {
        const double CentreX = 50;
        const double HalfWidth = 32;
        const double HalfDepth = 16;
        var centreY = 62 - height / 2;

        var xl = F(CentreX - HalfWidth);
        var xr = F(CentreX + HalfWidth);
        var bottomY = centreY + height;
        var cy = F(centreY);
        var bottom = F(bottomY);

        return
            $"""<g stroke="{edge}" stroke-width="1.5" stroke-linejoin="round">""" +
            // The drum wall, closed by the front half of the base ellipse.
            $"""<path d="M{xl},{cy} L{xl},{bottom} A{F(HalfWidth)},{F(HalfDepth)} 0 0 0 {xr},{bottom} L{xr},{cy} Z" fill="{left}"/>""" +
            // A darker sliver down the right hand side, so the drum reads as round.
            $"""<path d="M{F(CentreX + HalfWidth * 0.45)},{F(centreY + HalfDepth * 0.85)} L{xr},{cy} L{xr},{bottom} A{F(HalfWidth)},{F(HalfDepth)} 0 0 1 {F(CentreX + HalfWidth * 0.45)},{F(bottomY + HalfDepth * 0.6)} Z" fill="{right}" stroke="none"/>""" +
            $"""<ellipse cx="{F(CentreX)}" cy="{cy}" rx="{F(HalfWidth)}" ry="{F(HalfDepth)}" fill="{top}"/>""" +
            "</g>";
    }

    /// <summary>Three thin plates stacked on the tile.</summary>
    private static string Stack(string top, string left, string right, string edge)
    {
        var plates = new StringBuilder();

        // Drawn back to front so the upper plates overlap the ones below.
        for (var index = 2; index >= 0; index--)
        {
            plates.Append(CubeAt(top, left, right, edge, height: 7, baseY: 64 - index * 13));
        }

        return plates.ToString();
    }

    /// <summary>A ball resting on the tile.</summary>
    private static string Sphere(string top, string left, string right, string edge)
    {
        const double CentreX = 50;
        const double CentreY = 44;
        const double Radius = 26;

        return
            $"""<ellipse cx="{F(CentreX)}" cy="{F(CentreY + Radius + 4)}" rx="{F(Radius)}" ry="{F(Radius / 2)}" fill="{edge}" opacity="0.18"/>""" +
            $"""<circle cx="{F(CentreX)}" cy="{F(CentreY)}" r="{F(Radius)}" fill="{right}" stroke="{edge}" stroke-width="1.5"/>""" +
            $"""<path d="M{F(CentreX - Radius)},{F(CentreY)} a{F(Radius)},{F(Radius)} 0 0 1 {F(Radius * 2)},0 a{F(Radius)},{F(Radius / 2.6)} 0 0 1 -{F(Radius * 2)},0 Z" fill="{left}"/>""" +
            $"""<ellipse cx="{F(CentreX - Radius / 3)}" cy="{F(CentreY - Radius / 2.4)}" rx="{F(Radius / 2.6)}" ry="{F(Radius / 4)}" fill="{top}" opacity="0.75"/>""";
    }

    private static string CubeAt(string top, string left, string right, string edge, double height, double baseY)
    {
        const double CentreX = 50;
        const double HalfWidth = 32;
        const double HalfDepth = 16;

        var t = F(baseY - HalfDepth);
        var l = F(baseY);
        var b = F(baseY + HalfDepth);
        var lDrop = F(baseY + height);
        var bDrop = F(baseY + HalfDepth + height);

        var xl = F(CentreX - HalfWidth);
        var xr = F(CentreX + HalfWidth);
        var xc = F(CentreX);

        return
            $"""<g stroke="{edge}" stroke-width="1.5" stroke-linejoin="round">""" +
            $"""<path d="M{xc},{t} L{xr},{l} L{xc},{b} L{xl},{l} Z" fill="{top}"/>""" +
            $"""<path d="M{xl},{l} L{xc},{b} L{xc},{bDrop} L{xl},{lDrop} Z" fill="{left}"/>""" +
            $"""<path d="M{xc},{b} L{xr},{l} L{xr},{lDrop} L{xc},{bDrop} Z" fill="{right}"/>""" +
            "</g>";
    }

    private static string F(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>Blends two hex colours.</summary>
    /// <param name="color">The base colour.</param>
    /// <param name="towards">The colour to blend towards.</param>
    /// <param name="amount">How far to blend, from 0 to 1.</param>
    /// <returns>The blended colour, as a hex string.</returns>
    private static string Mix(string color, string towards, double amount)
    {
        var (r1, g1, b1) = Parse(color);
        var (r2, g2, b2) = Parse(towards);

        var r = (int)Math.Round(r1 + ((r2 - r1) * amount));
        var g = (int)Math.Round(g1 + ((g2 - g1) * amount));
        var b = (int)Math.Round(b1 + ((b2 - b1) * amount));

        return $"#{r:x2}{g:x2}{b:x2}";
    }

    private static (int R, int G, int B) Parse(string hex)
    {
        var value = hex.TrimStart('#');

        return (
            int.Parse(value[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }

    private static string Humanize(TopologyNodeKind kind) => kind switch
    {
        TopologyNodeKind.MessageBroker => "Message broker",
        TopologyNodeKind.ExternalService => "External service",
        _ => kind.ToString(),
    };
}
