using System.Diagnostics.CodeAnalysis;
using Aspire.Hosting.ApplicationModel;

namespace AspireTopology.Hosting.Classification;

/// <summary>
/// Decides what an Aspire resource is, semantically.
/// </summary>
/// <remarks>
/// Classifiers exist so that AspireTopology does not need one giant switch over every Aspire
/// integration type. New integrations are handled by adding a classifier, and anything nobody
/// recognises still reaches the diagram through the fallback classifier.
/// </remarks>
public interface ITopologyResourceClassifier
{
    /// <summary>Attempts to classify a resource.</summary>
    /// <param name="resource">The resource to classify.</param>
    /// <param name="descriptor">The classification, when this classifier recognised the resource.</param>
    /// <returns><see langword="true"/> when the resource was recognised.</returns>
    bool TryClassify(IResource resource, [NotNullWhen(true)] out TopologyNodeDescriptor? descriptor);
}
