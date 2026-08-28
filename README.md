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

```bash
aspire do topology
```

```text
artifacts/
└─ topology/
   ├─ topology.json
   └─ topology.isoflow.json
```

Open the result in the [viewer](viewer/AspireTopology.Viewer) for an interactive isometric diagram.

## In the dashboard

```csharp
builder.AddTopologyDiagram(options => options.Viewer = true);
```

`aspire run` now lists **topology** in the Aspire dashboard with a clickable URL, the way an
integration lists its management UI:

```text
Name       State     URLs
topology   Running   http://127.0.0.1:63304
```

The viewer is served from inside the AppHost. No container runtime, no Node.js, no second package:
its front end is embedded in `AspireTopology.Hosting`. It renders from the live
`DistributedApplicationModel` on every request rather than from a file, so what the dashboard links
to is never stale, and it exposes the raw model too:

```text
/                          the interactive diagram
/topology.json             the topology document
/topology.isoflow.json     the rendered Isoflow document
```

It binds to loopback on an OS-assigned port, and a failure to start is logged as a warning rather
than taking the app run down with it.

## Install

```bash
dotnet add package AspireTopology.Hosting
```

That is the only package an AppHost needs. It brings the topology model and the Isoflow renderer
with it.

## Options

```csharp
builder.AddTopologyDiagram(options =>
{
    options.OutputPath = "./docs/architecture";
    options.IncludeParameters = false;
    options.IncludeEndpoints = true;

    // Also refresh the files every time the AppHost starts, so F5 keeps the diagram current.
    options.GenerateOnStart = true;

    // List the interactive diagram in the Aspire dashboard, alongside your other resources.
    options.Viewer = true;
});
```

`GenerateOnStart` is off by default. `aspire do topology` is the deliberate way to produce
artifacts; turning this on means running the app refreshes them too. Both paths share the same
extractor and writer and produce identical files, and a failure to write on start is logged as a
warning rather than failing the run.

Describe individual resources with annotations, so the metadata travels with the resource instead
of living in a side file:

```csharp
api.WithTopologyMetadata(x =>
{
    x.DisplayName = "Public API";
    x.Group = "Backend";
});

secretThing.ExcludeFromTopology();
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

Two things are left out: parameters, unless `IncludeParameters` is set, and resources Aspire always
hides from its own dashboard. The latter is orchestration plumbing that only exists in run mode,
such as the project rebuilders behind hot reload, so the diagram is the same whether it came from
`aspire do topology` or from starting the app.

**AspireTopology never serializes secret values, environment values, parameter values, credentials
or connection strings.** Endpoints are fine. Connection strings are not.

## Layout is not topology

`topology.json` answers *what exists and how is it related*. It carries no coordinates. Where
things appear is a separate concern, so that a hand-tidied arrangement can survive regeneration:

```text
topology.json              generated
architecture.layout.json   human-owned
```

## Packages

| Package | For |
| --- | --- |
| `AspireTopology.Hosting` | AppHosts. This is the one to install. |
| `AspireTopology` | The neutral model. Depends on nothing. |
| `AspireTopology.Isoflow` | The Isoflow renderer. |

## Repository

```text
src/         the three libraries
tests/       unit tests and renderer snapshots
samples/     a sample AppHost with a project, a database, a cache and a front end
viewer/      a React viewer built on the Isoflow component
docs/        architecture, model and renderer authoring notes
```

The viewer is in the solution as an `.esproj`, so Visual Studio shows it in Solution Explorer and
F5 on it starts the Vite dev server. The .NET build never runs npm: the viewer's `dist` output is
checked in and embedded by `AspireTopology.Hosting`, which is what lets the package ship the front
end without forcing Node.js on anyone building the repo. Rebuild it and commit the result when the
viewer changes:

```bash
npm run build --prefix viewer/AspireTopology.Viewer
```

## Build it

```bash
dotnet build AspireTopology.slnx
```

```bash
dotnet test AspireTopology.slnx
```

Try the whole thing end to end:

```bash
cd samples/AspireTopology.Sample/AspireTopology.Sample.AppHost && aspire do topology
```

## Roadmap

- **v0.1 — Static topology.** Extraction, classification, references, `WaitFor`, endpoints,
  containment, `topology.json`, Isoflow JSON, `aspire do topology`, viewer. *(current)*
- **v0.2 — Better diagrams.** Icon selection, grouping, layout persistence, custom labels,
  include and exclude rules, a Mermaid renderer.
- **v0.3 — Runtime topology.** Resource state, health, allocated endpoints, replicas.
- **v0.4 — Observed topology.** OpenTelemetry relationships, request rates, architecture diffs, CI
  validation.

## Requirements

.NET SDK 10.0.100 or later, and Aspire 13.5 or later.

## Documentation

- [Architecture](docs/architecture.md)
- [The topology model](docs/topology-model.md)
- [Writing a renderer](docs/renderer-authoring.md)

## License

MIT. Isoflow Community Edition is MIT licensed and is used by the viewer sample only.
