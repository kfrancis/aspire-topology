using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Classification;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// Reads an Aspire application model and produces a topology document.
/// </summary>
/// <remarks>
/// This is the only place in AspireTopology that understands Aspire. Everything downstream of it
/// works on the neutral model.
/// </remarks>
public sealed class AspireTopologyExtractor : ITopologyExtractor
{
    private readonly TopologyDiagramOptions _options;
    private readonly ResourceExtractor _resources;
    private readonly RelationshipExtractor _relationships;

    /// <summary>Creates an extractor.</summary>
    /// <param name="options">Extraction options. Defaults are used when <see langword="null"/>.</param>
    /// <param name="classifier">Decides what each resource is. Defaults are used when <see langword="null"/>.</param>
    public AspireTopologyExtractor(TopologyDiagramOptions? options = null, ResourceClassifier? classifier = null)
    {
        _options = options ?? new TopologyDiagramOptions();
        _resources = new ResourceExtractor(_options, classifier);
        _relationships = new RelationshipExtractor();
    }

    /// <inheritdoc />
    public TopologyDocument Extract(DistributedApplicationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var included = model.Resources
            .Where(_resources.ShouldInclude)
            .ToList();

        var nodes = included
            .Select(_resources.Extract)
            .OrderBy(node => node.Id, StringComparer.Ordinal)
            .ToList();

        var edges = _relationships.Extract(included)
            .Select(relationship => new TopologyEdge(
                EdgeId(relationship),
                relationship.SourceId,
                relationship.TargetId,
                relationship.Kind))
            .ToList();

        var groups = BuildGroups(nodes, edges);

        return new TopologyDocument(_options.DocumentName ?? "Application", nodes, edges, groups);
    }

    private static string EdgeId(TopologyRelationship relationship) =>
        $"{relationship.SourceId}--{relationship.Kind.ToString().ToLowerInvariant()}--{relationship.TargetId}";

    private static List<TopologyGroup> BuildGroups(
        IReadOnlyList<TopologyNode> nodes,
        IReadOnlyList<TopologyEdge> edges)
    {
        var groups = new List<TopologyGroup>();

        // Containment: every node that other nodes name as their parent.
        var children = edges
            .Where(edge => edge.Kind is TopologyEdgeKind.Parent)
            .GroupBy(edge => edge.TargetId, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal);

        foreach (var parent in children)
        {
            var memberIds = parent
                .Select(edge => edge.SourceId)
                .Append(parent.Key)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            var name = nodes.FirstOrDefault(node => string.Equals(node.Id, parent.Key, StringComparison.Ordinal))?.Name
                ?? parent.Key;

            groups.Add(new TopologyGroup($"contains-{parent.Key}", name, TopologyGroupKind.Containment, memberIds));
        }

        // Logical: whatever the author put in a topology metadata annotation.
        var logical = nodes
            .Where(node => node.Properties.TryGetValue(TopologyPropertyNames.Group, out var value) && value is string)
            .GroupBy(node => (string)node.Properties[TopologyPropertyNames.Group]!, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal);

        foreach (var group in logical)
        {
            var memberIds = group
                .Select(node => node.Id)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            groups.Add(new TopologyGroup($"group-{group.Key}", group.Key, TopologyGroupKind.Logical, memberIds));
        }

        return groups;
    }
}
