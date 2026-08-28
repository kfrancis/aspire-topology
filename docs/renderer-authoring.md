# Writing a renderer

A renderer turns a `TopologyDocument` into a concrete format. It depends on `AspireTopology` only.

```csharp
public interface ITopologyRenderer
{
    string Name { get; }
    string FileExtension { get; }

    ValueTask RenderAsync(
        TopologyDocument topology,
        Stream output,
        CancellationToken cancellationToken = default);
}
```

`Name` becomes part of the generated file name: a renderer named `mermaid` with extension `.md`
writes `topology.mermaid.md` next to `topology.json`.

## Register it

```csharp
builder.AddTopologyDiagram(options =>
{
    options.Renderers.Add(new MermaidTopologyRenderer());
});
```

`Renderers` starts with the Isoflow renderer. Clear it to replace rather than add.

## Rules

**Be deterministic.** Generated artifacts land in source control. The same topology must produce
the same bytes. Sort anything whose order is not already meaningful; the extractor already sorts
nodes and edges.

**Do not put layout in the model.** Ask an `ITopologyLayoutEngine` for positions, or compute your
own. `LayeredTopologyLayoutEngine` is a reasonable default.

```csharp
var layout = new LayeredTopologyLayoutEngine().Layout(topology);
var position = layout.Find(node.Id);
```

**Handle `Unknown`.** Any node kind can appear, including kinds added after your renderer was
written. Pick a neutral shape rather than throwing.

**Read the property bag defensively.** Keys are optional and values are `object?`.

## Model the target format

Define the target's contract as records in your own project rather than building anonymous objects
inline. `AspireTopology.Isoflow.Model.IsoflowDocument` is the worked example: when Isoflow changes
its schema, one file changes.

## Test it

Two layers, both worth having:

1. Structural tests, over the mapped object graph. Every node became an item, every edge became a
   connector, every referenced icon was declared.
2. Golden tests, over the serialized output, with snapshots under `tests/snapshots/`. These catch
   the regressions structural assertions miss, such as a tile shifting by one.

Regenerate snapshots after an intentional change and review the diff:

```bash
ASPIRETOPOLOGY_UPDATE_SNAPSHOTS=1 dotnet test tests/AspireTopology.Isoflow.Tests
```
