namespace AspireTopology.Model;

/// <summary>
/// Semantic classification of a topology node.
/// </summary>
/// <remarks>
/// These values are deliberately independent of any orchestrator's resource types. A renderer
/// picks an icon and a shape from the kind; it never needs to know which Aspire integration
/// produced the node.
/// </remarks>
public enum TopologyNodeKind
{
    /// <summary>The node kind could not be determined.</summary>
    Unknown = 0,

    /// <summary>An application service, such as a project that exposes endpoints.</summary>
    Service,

    /// <summary>A relational or document database, or a database contained by one.</summary>
    Database,

    /// <summary>An in-memory cache.</summary>
    Cache,

    /// <summary>A message broker or event bus.</summary>
    MessageBroker,

    /// <summary>Blob, file, queue or table storage.</summary>
    Storage,

    /// <summary>A container that has no more specific classification.</summary>
    Container,

    /// <summary>An executable process that has no more specific classification.</summary>
    Executable,

    /// <summary>A service outside of the application that is referenced by it.</summary>
    ExternalService,

    /// <summary>A configuration parameter.</summary>
    Parameter,
}
