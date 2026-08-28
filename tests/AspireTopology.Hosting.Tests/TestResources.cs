using System.Net.Sockets;
using Aspire.Hosting.ApplicationModel;

namespace AspireTopology.Hosting.Tests;

/// <summary>
/// Stand-ins for the resources the Aspire integration packages ship.
/// </summary>
/// <remarks>
/// These are named the way the real integration resources are named. That is exactly what the
/// classifier matches on, so testing against them exercises the real path without dragging every
/// integration package into the test project.
/// </remarks>
internal sealed class PostgresResource(string name) : ContainerResource(name);

internal sealed class PostgresDatabaseResource(string name, IResource parent)
    : Resource(name), IResourceWithParent
{
    public IResource Parent { get; } = parent;
}

internal sealed class RedisResource(string name) : ContainerResource(name);

internal sealed class RabbitMQResource(string name) : ContainerResource(name);

internal sealed class AzureStorageResource(string name) : Resource(name);

/// <summary>A resource type AspireTopology has never seen.</summary>
internal sealed class SomeFutureResource(string name) : Resource(name);

internal static class TestResourceExtensions
{
    public static T WithReferenceTo<T>(this T resource, IResource target)
        where T : IResource
    {
        resource.Annotations.Add(new ResourceRelationshipAnnotation(target, "Reference"));
        return resource;
    }

    public static T WaitingFor<T>(this T resource, IResource target)
        where T : IResource
    {
        resource.Annotations.Add(new WaitAnnotation(target, WaitType.WaitUntilHealthy));
        return resource;
    }

    public static T WithEndpoint<T>(this T resource, string name, string scheme, int? port = null, int? targetPort = null, bool external = false)
        where T : IResource
    {
        resource.Annotations.Add(new EndpointAnnotation(
            ProtocolType.Tcp,
            uriScheme: scheme,
            transport: scheme,
            name: name,
            port: port,
            targetPort: targetPort,
            isExternal: external));

        return resource;
    }

    public static T WithImage<T>(this T resource, string image, string tag)
        where T : IResource
    {
        resource.Annotations.Add(new ContainerImageAnnotation { Image = image, Tag = tag });
        return resource;
    }
}
