namespace Transcriber.Server;

public class TextSegment
{
    public int Id { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public string Text { get; set; } = null!;
    public Episode Episode { get; set; } = null!;
}

public class Episode
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int Number { get; set; }
    public DateOnly AirDate { get; set; }
    public List<TextSegment> TextSegments { get; set; } = new();
}