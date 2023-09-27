using System.Text.Json.Serialization;

namespace Transcriber.Server;

public class Ingester
{
    private readonly MyContext _context;

    public Ingester(MyContext context)
    {
        _context = context;
    }

    public async Task IngestAsync(Ingestion ingestion)
    {
        var episode = _context
            .Episodes
            .SingleOrDefault(e => e.Number == ingestion.Episode);

        if (episode == null)
            episode = new Episode
            {
                Title = ingestion.Title,
                Number = ingestion.Episode,
                AirDate = ingestion.Date
            };

        foreach (var segment in ingestion.Segments)
        {
            var textSegment = new TextSegment
            {
                Start = TimeSpan.FromSeconds(segment.Start),
                End = TimeSpan.FromSeconds(segment.End),
                Text = segment.Text,
                Episode = episode
            };
            _context.TextSegments.Add(textSegment);
        }

        await _context.SaveChangesAsync();
    }
}

public class IngestionSegments
{
    [JsonPropertyName("start")] public double Start { get; set; }
    [JsonPropertyName("end")] public double End { get; set; }
    [JsonPropertyName("text")] public string Text { get; set; }
}

public class Ingestion
{
    [JsonPropertyName("episode_number")] public int Episode { get; set; }
    [JsonPropertyName("episode_date")] public DateOnly Date { get; set; }
    [JsonPropertyName("episode_title")] public string Title { get; set; }
    [JsonPropertyName("segments")] public List<IngestionSegments> Segments { get; set; } = null!;
}