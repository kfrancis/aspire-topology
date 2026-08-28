using System.Collections.Frozen;
using System.Reflection;

namespace AspireTopology.Hosting.Viewer;

/// <summary>
/// The viewer's built front end, embedded in this assembly.
/// </summary>
/// <remarks>
/// Embedding the built assets means an AppHost needs no container runtime, no Node.js and no
/// second package to show the diagram. The assets are produced by
/// <c>viewer/AspireTopology.Viewer</c> and checked in under its <c>dist</c> directory, so a build
/// of this package never depends on npm.
/// </remarks>
internal static class TopologyViewerAssets
{
    private const string Prefix = "viewer/";

    private static readonly FrozenDictionary<string, string> ResourceNames = BuildIndex();

    /// <summary>Whether a built front end was embedded at build time.</summary>
    public static bool Any => ResourceNames.Count > 0;

    /// <summary>The entry document, used for the root path and as the fallback.</summary>
    public const string IndexPath = "index.html";

    /// <summary>Opens an embedded asset.</summary>
    /// <param name="path">Request path, without a leading slash.</param>
    /// <returns>The asset stream, or <see langword="null"/> when there is no such asset.</returns>
    public static Stream? Open(string path) =>
        ResourceNames.TryGetValue(Normalize(path), out var resourceName)
            ? typeof(TopologyViewerAssets).Assembly.GetManifestResourceStream(resourceName)
            : null;

    /// <summary>Maps a file extension to a content type.</summary>
    /// <param name="path">The asset path.</param>
    /// <returns>The content type.</returns>
    public static string ContentTypeFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".js" or ".mjs" => "text/javascript; charset=utf-8",
        ".css" => "text/css; charset=utf-8",
        ".json" or ".map" => "application/json; charset=utf-8",
        ".svg" => "image/svg+xml",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".ico" => "image/x-icon",
        ".woff" => "font/woff",
        ".woff2" => "font/woff2",
        ".ttf" => "font/ttf",
        _ => "application/octet-stream",
    };

    private static string Normalize(string path) =>
        path.Replace('\\', '/').TrimStart('/');

    private static FrozenDictionary<string, string> BuildIndex()
    {
        var assembly = typeof(TopologyViewerAssets).Assembly;
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                index[Normalize(name[Prefix.Length..])] = name;
            }
        }

        return index.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }
}
