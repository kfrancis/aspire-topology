using AspireTopology.Model;

namespace AspireTopology.Hosting.Classification;

/// <summary>
/// What a classifier decided about a resource.
/// </summary>
/// <param name="Kind">The semantic classification.</param>
/// <param name="Icon">Optional renderer-specific icon hint.</param>
public sealed record TopologyNodeDescriptor(TopologyNodeKind Kind, string? Icon = null);
