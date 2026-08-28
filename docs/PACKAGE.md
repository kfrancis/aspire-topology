# AspireTopology

**Generate architecture topology and diagrams directly from your Aspire AppHost.**

Instead of parsing source code or maintaining architecture diagrams by hand, AspireTopology reads
Aspire's `DistributedApplicationModel` and converts resources, references, dependencies, endpoints
and containment relationships into a renderer-independent topology model. That topology is then
rendered as an interactive Isoflow diagram, or exported to other formats.

## Use it

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var db = postgres.AddDatabase("app");

var api = builder.AddProject<Projects.Api>("api")
    .WithReference(db);

builder.AddTopologyDiagram();

builder.Build().Run();
```

Run the app and the diagram appears in the Aspire dashboard as a **topology** resource, served from
inside the AppHost: no container runtime, no Node.js, no second package. Or produce the files on
demand:

```bash
aspire do topology
```

```text
artifacts/
└─ topology/
   ├─ topology.json
   └─ topology.isoflow.json
```

## Options

```csharp
builder.AddTopologyDiagram(options =>
{
    options.OutputPath = "./docs/architecture";
});
```

| Option | Default | |
| --- | --- | --- |
| `OutputPath` | `artifacts/topology` | Relative to the AppHost directory unless absolute. |
| `FileName` | `topology` | Base name of the generated files. |
| `GenerateOnStart` | `true` | Refresh the files every time the AppHost starts. |
| `Viewer` | `true` | List the interactive diagram in the Aspire dashboard. |
| `ViewerResourceName` | `topology` | The dashboard row's name. |
| `IncludeEndpoints` | `true` | Copy declared endpoints into node properties. |
| `IncludeParameters` | `false` | Parameters are configuration, not architecture. |

`GenerateOnStart` and `Viewer` only ever apply in run mode, so publish and deploy are untouched,
and neither can fail a run: a write error or a viewer that will not start is logged as a warning.

Describe individual resources with annotations, so the metadata travels with the resource instead
of living in a side file:

```csharp
api.WithTopologyMetadata(x =>
{
    x.DisplayName = "Public API";
    x.Group = "Backend";
});
```

## What it extracts

| From Aspire | Becomes |
| --- | --- |
| `ProjectResource` | `Service` node |
| Postgres, SQL Server, Mongo, Cosmos, … | `Database` node |
| Redis, Valkey, Garnet | `Cache` node |
| RabbitMQ, Kafka, Service Bus, Event Hubs | `MessageBroker` node |
| Blob, file, queue and table storage | `Storage` node |
| Anything else | `Container`, `Executable`, `Parameter` or `Unknown` node |
| `WithReference` | `Reference` edge |
| `WaitFor` | `Dependency` edge |
| `AddDatabase` and other child resources | `Parent` edge, plus a containment group |
| Declared endpoints | Node properties |

A resource type AspireTopology has never seen still appears on the diagram, as `Unknown`, with its
Aspire type recorded. Resources are never silently dropped.

**AspireTopology never serializes secret values, environment values, parameter values, credentials
or connection strings.** Endpoints are fine. Connection strings are not.

## Packages

| Package | For |
| --- | --- |
| `AspireTopology.Hosting` | AppHosts. This is the one to install. |
| `AspireTopology` | The neutral model. Depends on nothing. |
| `AspireTopology.Isoflow` | The Isoflow renderer. |

## Requirements

.NET SDK 10.0.100 or later, and Aspire 13.5 or later.

Screenshots, architecture notes and the sample AppHost are on GitHub:
<https://github.com/kfrancis/aspire-topology>
