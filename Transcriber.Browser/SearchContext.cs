using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Transcriber.Server;

namespace Transcriber.Browser;

public class SearchContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SearchContext(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.LogTo(Console.WriteLine);

        var databasePath = _httpContextAccessor.HttpContext?
            .Session
            .GetString("path");

        if (databasePath is null)
            return;

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        };
        optionsBuilder.UseSqlite(connectionString.ToString());
    }


    public DbSet<TextSegment> TextSegments => Set<TextSegment>();
    public DbSet<Episode> Episodes => Set<Episode>();
}