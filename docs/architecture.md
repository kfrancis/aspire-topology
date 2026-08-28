# Architecture

AspireTopology treats Aspire as a source, a neutral topology model as the product, and Isoflow as
the first renderer.

```text
Aspire DistributedApplicationModel
              │
              ▼
      AspireTopology.Hosting
              │
              ▼
        TopologyDocument
              │
       ┌──────┴────────┐
       ▼               ▼
AspireTopology      AspireTopology
   .Isoflow           .Mermaid
       │
       ▼
   Isoflow JSON
```

## The rule that matters

**`AspireTopology` never references `Aspire.Hosting`, and never references Isoflow.**

The core project describes what exists and how it is related. It does not know what produced the
topology or what will draw it. Every other decision in this repository follows from keeping that
boundary intact.

## Projects

| Project | Depends on | Responsibility |
| --- | --- | --- |
| `AspireTopology` | nothing | The topology model, layout abstractions, JSON serialization, the renderer interface. |
| `AspireTopology.Hosting` | `Aspire.Hosting`, `AspireTopology`, `AspireTopology.Isoflow` | Reads the Aspire application model, classifies resources, discovers relationships, registers the `topology` pipeline step, writes artifacts. |
| `AspireTopology.Isoflow` | `AspireTopology` | Maps a topology onto Isoflow `initialData`. |
| `AspireTopology.Viewer` | Isoflow, React | Loads the generated JSON and renders it. |

`AspireTopology.Hosting` depending on `AspireTopology.Isoflow` is a deliberate v0.1 shortcut, so
that `AddTopologyDiagram()` produces a diagram with no second package to install. If a cleaner
separation becomes worth it, the renderer moves to an `AspireTopology.Hosting.Isoflow` package and
`TopologyDiagramOptions.Renderers` starts empty.

## Topology and layout are separate

`TopologyDocument` carries no coordinates. Layout is a separate `TopologyLayout`, computed by an
`ITopologyLayoutEngine`.

```text
TopologyDocument     what exists and how it is related
TopologyLayout       where it should appear
```

Isoflow wants an isometric grid, Mermaid wants automatic graph layout, an SVG export wants pixels,
and a human wants their own arrangement to survive regeneration. None of that belongs in the model.

## Two ways in, one way through

```text
aspire do topology  ──►  Pipeline/TopologyPipelineStep   ─┐
                                                          ├─►  extractor ──► writer
AppHost start       ──►  Startup/TopologyStartupGeneration ┘   (opt-in, run mode only)
```

Both entry points share the extractor and the writer, so they produce identical files. The startup
path is opt-in through `TopologyDiagramOptions.GenerateOnStart`, is registered only in run mode,
and downgrades write failures to a warning: a diagram is not worth failing an app run over.

Run mode adds orchestration resources the pipeline never sees. Those that Aspire marks
`HiddenBehavior.Always` are filtered out during extraction, which is what keeps the two paths in
agreement.

## The viewer

`TopologyDiagramOptions.Viewer` adds a `TopologyViewerResource` to the application model and starts
a small Kestrel server inside the AppHost process. `TopologyViewerService` publishes the resource's
state and URL through `ResourceNotificationService`, which is what makes it appear in the dashboard
list next to everything else.

```text
TopologyViewerResource      no process, no container, IResourceWithoutLifetime
TopologyViewerService       IHostedService: serves the front end, publishes state and URL
TopologyViewerAssets        the built front end, embedded in the assembly
```

Three consequences worth knowing:

- **It renders from the live model, not from disk.** `/topology.json` and `/topology.isoflow.json`
  run the extractor per request. Nothing can go stale, and runtime state has an obvious home later.
- **It excludes itself.** The resource carries a `TopologyMetadataAnnotation { Exclude = true }`,
  so the viewer never shows up in its own diagram.
- **It cannot break a run.** Failures are logged and published as `FailedToStart`; the app carries
  on without a diagram.

The front end is built by `viewer/AspireTopology.Viewer` and its `dist` output is checked in, then
embedded by an MSBuild glob. That is a deliberate trade: a checked-in build artifact, in exchange
for a .NET build and a NuGet package that never depend on Node.js. `ViewerAssetTests` fails loudly
if the glob ever stops matching.

## The experimental pipeline API

Aspire marks its pipeline API experimental (`ASPIREPIPELINES001`). Every use of it lives in
[`Pipeline/TopologyPipelineStep.cs`](../src/AspireTopology.Hosting/Pipeline/TopologyPipelineStep.cs)
so that a breaking change in a future Aspire release touches one small file.

## What is never serialized

AspireTopology never writes secret values, environment values, parameter values, credentials or
connection strings into a topology. Endpoints are fine. This is enforced by only reading the
annotations that are safe, and covered by `SecretSafetyTests`.
