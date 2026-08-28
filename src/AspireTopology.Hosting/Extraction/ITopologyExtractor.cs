using Aspire.Hosting.ApplicationModel;
using AspireTopology.Model;

namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// Converts an Aspire application model into a renderer-independent topology.
/// </summary>
public interface ITopologyExtractor
{
    /// <summary>Extracts a topology from an Aspire application model.</summary>
    /// <param name="model">The Aspire application model.</param>
    /// <returns>The topology.</returns>
    TopologyDocument Extract(DistributedApplicationModel model);
}
