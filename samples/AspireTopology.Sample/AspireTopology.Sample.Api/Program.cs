var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "AspireTopology sample API");
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
