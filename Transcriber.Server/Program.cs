using Transcriber.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MyContext>();
builder.Services.AddScoped<Ingester>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<MyContext>()
        .Database
        .EnsureCreated();
}


app.MapGet("/", () => "Hello World!");

app.MapGet("/exists", 
    (int episode, Ingester ingester) => 
        ingester.CheckEpisodeExists(episode));

app.MapPost("/ingest",
    async (
            Ingester ingester,
            Ingestion ingestion) =>
        await ingester.IngestAsync(ingestion));

app.Run();