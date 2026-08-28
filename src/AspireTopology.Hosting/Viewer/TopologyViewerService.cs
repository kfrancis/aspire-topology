using Aspire.Hosting.ApplicationModel;
using AspireTopology.Hosting.Extraction;
using AspireTopology.Isoflow;
using AspireTopology.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspireTopology.Hosting.Viewer;

/// <summary>
/// Serves the topology viewer from inside the AppHost and publishes it to the dashboard.
/// </summary>
/// <remarks>
/// The viewer renders the topology from the live <see cref="DistributedApplicationModel"/> on each
/// request rather than from a file on disk, so what the dashboard links to is never stale. That is
/// also the seam runtime state plugs into later: the same request already has the live model.
/// </remarks>
internal sealed class TopologyViewerService : IHostedService, IAsyncDisposable
{
    private readonly TopologyViewerResource _resource;
    private readonly TopologyDiagramOptions _options;
    private readonly DistributedApplicationModel _model;
    private readonly ResourceNotificationService _notifications;
    private readonly ILogger<TopologyViewerService> _logger;
    private readonly ITopologyExtractor _extractor;
    private readonly IsoflowTopologyRenderer _renderer = new();

    private WebApplication? _app;

    public TopologyViewerService(
        TopologyViewerResource resource,
        TopologyDiagramOptions options,
        DistributedApplicationModel model,
        ResourceNotificationService notifications,
        ILogger<TopologyViewerService> logger)
    {
        _resource = resource;
        _options = options;
        _model = model;
        _notifications = notifications;
        _logger = logger;
        _extractor = new AspireTopologyExtractor(options);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _app = BuildApplication();
            await _app.StartAsync(cancellationToken).ConfigureAwait(false);

            var url = _app.Urls.FirstOrDefault() ?? throw new InvalidOperationException("The viewer server did not bind a URL.");

            await PublishAsync(KnownResourceStates.Running, url, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Topology viewer listening on {Url}", url);
        }
        catch (Exception exception)
        {
            // A diagram is not worth failing an app run over.
            _logger.LogWarning(exception, "Could not start the topology viewer.");
            await PublishAsync(KnownResourceStates.FailedToStart, url: null, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_app is not null)
        {
            await _app.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync().ConfigureAwait(false);
        }
    }

    private WebApplication BuildApplication()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.Logging.ClearProviders();

        var app = builder.Build();

        // Port 0 asks the OS for a free port. Loopback only: this is a developer tool, and the
        // AppHost's own configuration must not decide where it binds.
        app.Urls.Clear();
        app.Urls.Add("http://127.0.0.1:0");

        app.MapGet("/topology.json", () => Results.Text(
            TopologyJson.Serialize(_extractor.Extract(_model)),
            "application/json; charset=utf-8"));

        app.MapGet("/topology.isoflow.json", () => Results.Text(
            _renderer.RenderToString(_extractor.Extract(_model)),
            "application/json; charset=utf-8"));

        app.MapGet("/{**path}", (string? path) => ServeAsset(path));

        return app;
    }

    private static IResult ServeAsset(string? path)
    {
        if (!TopologyViewerAssets.Any)
        {
            return Results.Content(PlaceholderPage, "text/html; charset=utf-8");
        }

        var requested = string.IsNullOrEmpty(path) ? TopologyViewerAssets.IndexPath : path;

        if (TopologyViewerAssets.Open(requested) is { } asset)
        {
            return Results.Stream(asset, TopologyViewerAssets.ContentTypeFor(requested));
        }

        // Single page app: unknown routes serve the entry document so client-side routing works.
        // A missing file extension is the signal that this is a route rather than a lost asset.
        return TopologyViewerAssets.Open(TopologyViewerAssets.IndexPath) is { } index
            ? Results.Stream(index, TopologyViewerAssets.ContentTypeFor(TopologyViewerAssets.IndexPath))
            : Results.NotFound();
    }

    private async Task PublishAsync(string state, string? url, CancellationToken cancellationToken)
    {
        try
        {
            await _notifications.PublishUpdateAsync(_resource, snapshot => snapshot with
            {
                ResourceType = TopologyViewerResource.ResourceTypeName,
                State = state,
                StartTimeStamp = snapshot.StartTimeStamp ?? DateTime.UtcNow,
                Urls = url is null
                    ? []
                    : [new UrlSnapshot(Name: "viewer", Url: url, IsInternal: false)],
                Properties = url is null
                    ? snapshot.Properties
                    : snapshot.Properties
                        .Add(new ResourcePropertySnapshot("topology.endpoint", $"{url}topology.json"))
                        .Add(new ResourcePropertySnapshot("topology.renderer", _renderer.Name)),
            }).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogDebug(exception, "Could not publish the topology viewer state.");
        }
    }

    private const string PlaceholderPage =
        """
        <!doctype html>
        <meta charset="utf-8">
        <title>AspireTopology</title>
        <body style="font-family: system-ui, sans-serif; margin: 3rem; max-width: 40rem">
        <h1>Topology viewer front end not embedded</h1>
        <p>This build of AspireTopology.Hosting was produced without the viewer assets.</p>
        <p>The topology itself is still available:</p>
        <ul>
          <li><a href="/topology.json">topology.json</a></li>
          <li><a href="/topology.isoflow.json">topology.isoflow.json</a></li>
        </ul>
        </body>
        """;
}
