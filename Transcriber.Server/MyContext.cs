using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// ReSharper disable CollectionNeverUpdated.Global

namespace Transcriber.Server;

public class MyContext : DbContext
{
    private readonly ILogger<MyContext> _logger;

    public MyContext(ILogger<MyContext> logger) => 
        _logger = logger;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = "hss-texts.sqlite3",
            Pooling = false
        };
        optionsBuilder.UseSqlite(connectionString.ToString());
    }

  

    public DbSet<TextSegment> TextSegments => Set<TextSegment>();
    public DbSet<Episode> Episodes => Set<Episode>();
}

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