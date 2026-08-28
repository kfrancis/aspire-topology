namespace AspireTopology.Isoflow.Tests;

/// <summary>
/// Reads and updates the golden files under <c>tests/snapshots</c>.
/// </summary>
/// <remarks>
/// Set <c>ASPIRETOPOLOGY_UPDATE_SNAPSHOTS=1</c> to rewrite them after an intentional renderer
/// change, then review the diff.
/// </remarks>
internal static class SnapshotFile
{
    private const string UpdateVariable = "ASPIRETOPOLOGY_UPDATE_SNAPSHOTS";

    public static bool ShouldUpdate =>
        Environment.GetEnvironmentVariable(UpdateVariable) is "1" or "true";

    public static string Directory => Path.Combine(RepositoryRoot(), "tests", "snapshots");

    public static string Read(string name)
    {
        var path = Path.Combine(Directory, name);
        return File.Exists(path) ? Normalize(File.ReadAllText(path)) : string.Empty;
    }

    public static void Write(string name, string content)
    {
        System.IO.Directory.CreateDirectory(Directory);
        File.WriteAllText(Path.Combine(Directory, name), Normalize(content));
    }

    public static string Normalize(string content) =>
        content.ReplaceLineEndings("\n").TrimEnd() + "\n";

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AspireTopology.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find the repository root above {AppContext.BaseDirectory}.");
    }
}
