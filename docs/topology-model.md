# The topology model

Four concepts: documents, nodes, edges and groups.

```csharp
public sealed record TopologyDocument(
    string Name,
    IReadOnlyList<TopologyNode> Nodes,
    IReadOnlyList<TopologyEdge> Edges,
    IReadOnlyList<TopologyGroup> Groups);
```

## Nodes

Node kinds are semantic, not Aspire types. A renderer picks an icon from the kind and never needs
to know which integration produced the node.

| Kind | Meaning |
| --- | --- |
| `Service` | An application service, such as a project with endpoints. |
| `Database` | A database server, or a database inside one. |
| `Cache` | An in-memory cache. |
| `MessageBroker` | A message broker or event bus. |
| `Storage` | Blob, file, queue or table storage. |
| `Container` | A container with no more specific classification. |
| `Executable` | A process with no more specific classification. |
| `ExternalService` | A service outside the application. |
| `Parameter` | A configuration parameter. |
| `Unknown` | Nothing recognised it. |

`Unknown` is a feature. A resource type AspireTopology has never seen still appears, with its
Aspire type recorded in the properties. Resources are never silently dropped.

## Edges

| Kind | Meaning |
| --- | --- |
| `Reference` | The source was given the target's connection information (`WithReference`). |
| `Dependency` | The source waits for the target before starting (`WaitFor`). |
| `Parent` | The target contains the source, such as a database inside a server. |
| `Observed` | Seen at runtime rather than declared. Reserved for v0.4. |

`Reference` deliberately does not mean "calls". Aspire's `WithReference` hands over connection
information; whether a call is ever made is a runtime question. Once observed relationships arrive
the vocabulary grows, without changing what extraction means today:

```text
Reference
StartupDependency
Endpoint
ObservedHttp
ObservedGrpc
ObservedMessaging
```

## Properties

Every node carries a flexible metadata bag so renderers can use information the model does not
formally expose yet.

```json
{
  "aspire.name": "postgres",
  "aspire.resourceType": "PostgresServerResource",
  "container.image": "postgres",
  "endpoint.tcp.targetPort": 5432
}
```

Keys are namespaced. See
[`TopologyPropertyNames`](../src/AspireTopology.Hosting/Extraction/TopologyPropertyNames.cs) for the
ones the extractor writes.

Never in the bag: secret values, environment values, parameter values, credentials, connection
strings.

## Layout

Positions live outside the document.

```csharp
public sealed record TopologyLayout(
    IReadOnlyDictionary<string, TopologyPosition> Nodes);
```

`LayeredTopologyLayoutEngine` places nodes in layers by dependency depth: roots on top,
what they point at below. It is deterministic and terminates on cycles. Containment edges do not
contribute to depth, so a database is not pushed a layer below its own server.

`TopologyLayout.WithOverrides` merges a human-owned layout over a generated one, which is the
mechanism behind:

```text
topology.json              generated
architecture.layout.json   human-owned
```
