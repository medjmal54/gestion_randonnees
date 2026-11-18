using System.Security.Claims;
using Gestion_randonnees.Data;
using Gestion_randonnees.Models.Enums;
using Gestion_randonnees.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TryGetUserId(out var userId)) return Challenge();

        var participations = await _context.Participations
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Include(p => p.Hike)
            .OrderByDescending(p => p.DateCreation)
            .ToListAsync();

        var totalSpent = participations
            .Where(p => p.Statut == ParticipationStatus.Acceptee)
            .Sum(p => p.Hike?.Prix ?? 0m);

        var vm = new UserDashboardViewModel
        {
            Participations = participations,
            TotalSpent = totalSpent
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationsRead()
    {
        if (!TryGetUserId(out var userId)) return Challenge();

        var unseen = await _context.Participations
            .Where(p => p.UserId == userId && !p.IsSeenByUser && p.Statut != ParticipationStatus.EnAttente)
            .ToListAsync();

        foreach (var p in unseen)
            p.IsSeenByUser = true;

        if (unseen.Count > 0)
            await _context.SaveChangesAsync();

        return Ok(new { status = "ok" });
    }

    private bool TryGetUserId(out int userId)
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
