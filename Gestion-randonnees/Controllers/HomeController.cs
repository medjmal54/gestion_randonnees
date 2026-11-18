using Gestion_randonnees.Data;
using Gestion_randonnees.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var upcoming = await _context.Hikes
            .AsNoTracking()
            .Where(h => h.DateDebut >= now && h.Statut == HikeStatus.Prevue)
            .OrderBy(h => h.DateDebut)
            .Take(3)
            .ToListAsync();

        return View(upcoming);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
