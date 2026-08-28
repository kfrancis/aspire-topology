using Aspire.Hosting.ApplicationModel;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// Discovers the relationships between Aspire resources.
/// </summary>
/// <remarks>
/// Relationships come from what the application model actually declares: relationship annotations,
/// wait annotations and parent resources. Nothing is inferred from resource names, and a
/// reference is reported as a reference rather than as a call, because a reference only means
/// connection information was handed over.
/// </remarks>
public sealed class RelationshipExtractor
{
    /// <summary>The relationship annotation type Aspire writes for <c>WithReference</c>.</summary>
    public const string ReferenceRelationship = "Reference";

    /// <summary>The relationship annotation type Aspire writes for <c>WaitFor</c>.</summary>
    public const string WaitForRelationship = "WaitFor";

    /// <summary>The relationship annotation type Aspire writes for containment.</summary>
    public const string ParentRelationship = "Parent";

    /// <summary>Discovers every relationship between the given resources.</summary>
    /// <param name="resources">The resources that will appear in the topology.</param>
    /// <returns>The relationships, deduplicated and in a stable order.</returns>
    public IReadOnlyList<TopologyRelationship> Extract(IReadOnlyCollection<IResource> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);

        var known = resources.Select(resource => resource.Name).ToHashSet(StringComparer.Ordinal);
        var relationships = new List<TopologyRelationship>();

        foreach (var resource in resources)
        {
            foreach (var annotation in resource.Annotations.OfType<ResourceRelationshipAnnotation>())
            {
                relationships.Add(new TopologyRelationship(
                    resource.Name,
                    annotation.Resource.Name,
                    MapRelationshipType(annotation.Type)));
            }

            foreach (var wait in resource.Annotations.OfType<WaitAnnotation>())
            {
                relationships.Add(new TopologyRelationship(
                    resource.Name,
                    wait.Resource.Name,
                    TopologyEdgeKind.Dependency));
            }

            if (resource is IResourceWithParent withParent)
            {
                relationships.Add(new TopologyRelationship(
                    resource.Name,
                    withParent.Parent.Name,
                    TopologyEdgeKind.Parent));
            }
        }

        return relationships
            .Where(relationship => known.Contains(relationship.SourceId) && known.Contains(relationship.TargetId))
            .Where(relationship => !string.Equals(relationship.SourceId, relationship.TargetId, StringComparison.Ordinal))
            .Distinct()
            .OrderBy(relationship => relationship.SourceId, StringComparer.Ordinal)
            .ThenBy(relationship => relationship.TargetId, StringComparer.Ordinal)
            .ThenBy(relationship => relationship.Kind)
            .ToList();
    }

    /// <summary>
    /// Maps an Aspire relationship annotation type onto an edge kind. Unrecognised types are kept
    /// as references rather than dropped, so a new Aspire relationship still shows up.
    /// </summary>
    /// <param name="type">The annotation type string.</param>
    /// <returns>The edge kind.</returns>
    public static TopologyEdgeKind MapRelationshipType(string type) => type switch
    {
        WaitForRelationship => TopologyEdgeKind.Dependency,
        ParentRelationship => TopologyEdgeKind.Parent,
        _ => TopologyEdgeKind.Reference,
    };
}
