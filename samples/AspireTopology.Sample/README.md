# AspireTopology sample

A deliberately diverse but small AppHost: a project, a Postgres server, a database inside it, a
Redis cache, a web front end, references, a `WaitFor`, and endpoints.

```bash
cd AspireTopology.Sample.AppHost
aspire do topology
```

Artifacts land in `artifacts/topology/` at the repository root.

## Running the app

F5 in Visual Studio, or:

```bash
dotnet run --project AspireTopology.Sample.AppHost
```

The dashboard is on a fixed port from `Properties/launchSettings.json`:

```text
https://localhost:17110      dashboard (https profile)
http://localhost:15110       dashboard (http profile)
```

Resources: `api`, `web`, `postgres`, `appdb`, `cache`, and `topology` — the last being the viewer,
whose URL is in its dashboard row.

The project sets `AspireUseCliBundle=false` so the DCP orchestrator and dashboard binaries come in
as packages and the built AppHost runs on its own. With it set to `true`, those paths are resolved
from an installed Aspire CLI at build time, and F5 in Visual Studio fails at startup with
`Property CliPath` / `Property DashboardPath` errors, because it launches the built executable
directly rather than through the CLI. `ASPIRE010` warns about the trade and is suppressed here.

Running the app refreshes the same files and lists a **topology** resource in the dashboard, both
by default. Note that running the app needs a container runtime for Postgres and Redis, while
`aspire do topology` does not start anything.

## No ServiceDefaults

There is no `ServiceDefaults` project here. It adds OpenTelemetry, health checks and service
discovery to the referenced projects, none of which the extractor reads: topology comes from the
AppHost's application model alone. It becomes relevant at v0.3 (runtime state) and v0.4 (observed
relationships), and should be added then.
