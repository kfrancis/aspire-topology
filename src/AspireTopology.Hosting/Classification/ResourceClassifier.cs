using Aspire.Hosting.ApplicationModel;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Classification;

/// <summary>
/// Runs a list of classifiers in order and returns the first classification produced.
/// </summary>
public sealed class ResourceClassifier
{
    private readonly IReadOnlyList<ITopologyResourceClassifier> _classifiers;

    /// <summary>Creates a classifier chain.</summary>
    /// <param name="classifiers">
    /// The classifiers to run, most specific first. The last one should always succeed.
    /// </param>
    public ResourceClassifier(IReadOnlyList<ITopologyResourceClassifier> classifiers)
    {
        ArgumentNullException.ThrowIfNull(classifiers);
        _classifiers = classifiers;
    }

    /// <summary>Creates a classifier chain with the built-in classifiers.</summary>
    public ResourceClassifier()
        : this(CreateDefaultClassifiers())
    {
    }

    /// <summary>
    /// Builds the default chain: known integrations first, then the Aspire resource types, then a
    /// fallback that accepts everything.
    /// </summary>
    /// <returns>The classifiers, in order.</returns>
    public static IReadOnlyList<ITopologyResourceClassifier> CreateDefaultClassifiers() =>
    [
        new KnownIntegrationClassifier(),
        new ProjectResourceClassifier(),
        new ParameterResourceClassifier(),
        new ContainerResourceClassifier(),
        new ExecutableResourceClassifier(),
        new FallbackResourceClassifier(),
    ];

    /// <summary>Classifies a resource.</summary>
    /// <param name="resource">The resource to classify.</param>
    /// <returns>The classification.</returns>
    public TopologyNodeDescriptor Classify(IResource resource)
    {
        foreach (var classifier in _classifiers)
        {
            if (classifier.TryClassify(resource, out var descriptor))
            {
                return descriptor;
            }
        }

        return new TopologyNodeDescriptor(TopologyNodeKind.Unknown);
    }
}
