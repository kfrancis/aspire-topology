# AspireTopology.Viewer

Renders a generated `topology.isoflow.json` with the [Isoflow](https://isoflow.io) React component.

## Run it

```bash
cd samples/AspireTopology.Sample/AspireTopology.Sample.AppHost
aspire do topology
```

```bash
cd viewer/AspireTopology.Viewer
npm install
cp ../../artifacts/topology/topology.isoflow.json public/
npm run dev
```

Point it somewhere else with `VITE_TOPOLOGY_URL`.

## What it does

- Loads the generated Isoflow document.
- Merges `@isoflow/isopacks` icons over the self-contained SVG icons AspireTopology emits, so the
  diagram renders either way.
- Saves layout changes reported by Isoflow's `onModelUpdate` to local storage, applied on the next
  load.

That last part is the shape the project is designed around:

```text
topology.json              generated, overwritten every run
architecture.layout.json   human-owned, survives regeneration
```

Local storage is a placeholder for that file. Writing it to disk needs a small dev-server endpoint
and is deliberately out of scope for v0.1.

## Licensing

Isoflow Community Edition is MIT licensed. The viewer is a sample, not a published package.
