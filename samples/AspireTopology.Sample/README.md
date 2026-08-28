# AspireTopology sample

A deliberately diverse but small AppHost: a project, a Postgres server, a database inside it, a
Redis cache, a web front end, references, a `WaitFor`, and endpoints.

```bash
cd AspireTopology.Sample.AppHost
aspire do topology
```

Artifacts land in `artifacts/topology/` at the repository root.

The sample also sets `GenerateOnStart`, so `aspire run` (or F5) refreshes the same files, and
`Viewer`, so the dashboard lists a **topology** resource with a link to the interactive diagram.
Note that running the app needs a container runtime for Postgres and Redis, while
`aspire do topology` does not start anything.

## No ServiceDefaults

There is no `ServiceDefaults` project here. It adds OpenTelemetry, health checks and service
discovery to the referenced projects, none of which the extractor reads: topology comes from the
AppHost's application model alone. It becomes relevant at v0.3 (runtime state) and v0.4 (observed
relationships), and should be added then.
