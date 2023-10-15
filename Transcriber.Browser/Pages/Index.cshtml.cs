using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Transcriber.Server;

namespace Transcriber.Browser.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly SearchContext _context;

    public List<TextSegment>? ResultSegments { get; set; }

    public IndexModel(ILogger<IndexModel> logger, SearchContext context)
    {
        _logger = logger;
        _context = context;
    }


    public void OnGet()
    {
    }

    public void OnPostSearchFullText(string query)
    {
        ResultSegments = _context
            .TextSegments
            .Include(t => t.Episode)
            .Where(t =>
                EF.Functions.Like(t.Text, $"%{query}%"))
            .OrderBy(o => o.Episode.Number)
            .ToList();
    }

    public void OnPostSaveDbInfo(string path)
    {
        Console.WriteLine($"got path: {path}");
        HttpContext.Session.SetString("path", path);
    }
}