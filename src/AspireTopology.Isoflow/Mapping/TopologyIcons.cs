using System.Text;
using AspireTopology.Isoflow.Model;
using AspireTopology.Model;

namespace AspireTopology.Isoflow.Mapping;

/// <summary>
/// Supplies one icon per <see cref="TopologyNodeKind"/>.
/// </summary>
/// <remarks>
/// The icons are inline SVG data URIs so a generated diagram renders on its own, with no icon
/// pack installed and no network access. A viewer that has <c>@isoflow/isopacks</c> available can
/// override them by merging its own icon list ahead of these, keyed by the same identifiers.
/// </remarks>
public static class TopologyIcons
{
    /// <summary>Prefix shared by every built-in icon identifier.</summary>
    public const string IdPrefix = "aspire-topology-";

    /// <summary>Name of the collection the built-in icons belong to.</summary>
    public const string CollectionName = "AspireTopology";

    private const string Frame =
        """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="64" height="64"><rect x="2" y="2" width="60" height="60" rx="10" fill="{0}"/><g fill="none" stroke="#ffffff" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round">{1}</g></svg>""";

    private static readonly Dictionary<TopologyNodeKind, (string Color, string Glyph)> Glyphs = new()
    {
        [TopologyNodeKind.Service] = ("#2563eb", """<rect x="16" y="18" width="32" height="28" rx="4"/><path d="M24 30h16M24 38h10"/>"""),
        [TopologyNodeKind.Database] = ("#7c3aed", """<ellipse cx="32" cy="20" rx="16" ry="6"/><path d="M16 20v24c0 3.3 7.2 6 16 6s16-2.7 16-6V20"/><path d="M16 32c0 3.3 7.2 6 16 6s16-2.7 16-6"/>"""),
        [TopologyNodeKind.Cache] = ("#dc2626", """<path d="M34 12 20 36h12l-2 16 14-24H32z"/>"""),
        [TopologyNodeKind.MessageBroker] = ("#ea580c", """<rect x="14" y="20" width="36" height="24" rx="4"/><path d="m14 24 18 12 18-12"/>"""),
        [TopologyNodeKind.Storage] = ("#0891b2", """<rect x="14" y="16" width="36" height="12" rx="3"/><rect x="14" y="36" width="36" height="12" rx="3"/><path d="M22 22h.02M22 42h.02"/>"""),
        [TopologyNodeKind.Container] = ("#0d9488", """<path d="M32 12 12 22v20l20 10 20-10V22z"/><path d="m12 22 20 10 20-10M32 32v20"/>"""),
        [TopologyNodeKind.Executable] = ("#4b5563", """<rect x="12" y="16" width="40" height="32" rx="4"/><path d="m22 28 6 6-6 6M34 40h10"/>"""),
        [TopologyNodeKind.ExternalService] = ("#0369a1", """<circle cx="32" cy="32" r="18"/><path d="M14 32h36M32 14c5 6 5 30 0 36-5-6-5-30 0-36"/>"""),
        [TopologyNodeKind.Parameter] = ("#a16207", """<path d="M26 18h-6a4 4 0 0 0-4 4v6l-4 4 4 4v6a4 4 0 0 0 4 4h6M38 18h6a4 4 0 0 1 4 4v6l4 4-4 4v6a4 4 0 0 1-4 4h-6"/>"""),
        [TopologyNodeKind.Unknown] = ("#64748b", """<circle cx="32" cy="32" r="18"/><path d="M26 26a6 6 0 1 1 8 6v4M32 44h.02"/>"""),
    };

    /// <summary>Returns the icon identifier used for a node kind.</summary>
    /// <param name="kind">The node kind.</param>
    /// <returns>The icon identifier.</returns>
    public static string IdFor(TopologyNodeKind kind) =>
        IdPrefix + kind.ToString().ToLowerInvariant();

    /// <summary>Builds the icon definition for a node kind.</summary>
    /// <param name="kind">The node kind.</param>
    /// <returns>The icon.</returns>
    public static IsoflowIcon Create(TopologyNodeKind kind)
    {
        var (color, glyph) = Glyphs.TryGetValue(kind, out var entry) ? entry : Glyphs[TopologyNodeKind.Unknown];
        var svg = Frame.Replace("{0}", color, StringComparison.Ordinal).Replace("{1}", glyph, StringComparison.Ordinal);
        var url = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

        return new IsoflowIcon(IdFor(kind), kind.ToString(), url, CollectionName);
    }
}
