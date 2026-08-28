using Aspire.Hosting.ApplicationModel;

namespace AspireTopology.Hosting.Extraction;

/// <summary>
/// Copies the declared endpoints of a resource into node properties.
/// </summary>
/// <remarks>
/// Endpoints describe how a resource is reached and are safe to publish. Environment variables,
/// parameter values and connection strings are not, and are never read here.
/// </remarks>
public sealed class EndpointExtractor
{
    /// <summary>Writes endpoint properties for a resource.</summary>
    /// <param name="resource">The resource to read.</param>
    /// <param name="properties">The property bag to write into.</param>
    public void Extract(IResource resource, IDictionary<string, object?> properties)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(properties);

        var endpoints = resource.Annotations
            .OfType<EndpointAnnotation>()
            .OrderBy(endpoint => endpoint.Name, StringComparer.Ordinal);

        foreach (var endpoint in endpoints)
        {
            properties[TopologyPropertyNames.Endpoint(endpoint.Name, "scheme")] = endpoint.UriScheme;
            properties[TopologyPropertyNames.Endpoint(endpoint.Name, "transport")] = endpoint.Transport;
            properties[TopologyPropertyNames.Endpoint(endpoint.Name, "external")] = endpoint.IsExternal;

            if (endpoint.Port is { } port)
            {
                properties[TopologyPropertyNames.Endpoint(endpoint.Name, "port")] = port;
            }

            if (endpoint.TargetPort is { } targetPort)
            {
                properties[TopologyPropertyNames.Endpoint(endpoint.Name, "targetPort")] = targetPort;
            }
        }
    }
}
