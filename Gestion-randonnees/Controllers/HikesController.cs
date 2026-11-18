using System.Security.Claims;
using Gestion_randonnees.Data;
using Gestion_randonnees.Models.Entities;
using Gestion_randonnees.Models.Enums;
using Gestion_randonnees.ViewModels.Hikes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Controllers;

public class HikesController : Controller
{
    private readonly ApplicationDbContext _context;

    public HikesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] HikeFilterViewModel filter)
    {
        var query = _context.Hikes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Region))
        {
            var region = filter.Region.Trim();
            query = query.Where(h => h.Region.Contains(region));
        }

        if (!string.IsNullOrWhiteSpace(filter.Categorie))
            query = query.Where(h => h.Categorie == filter.Categorie);

        if (filter.Difficulte.HasValue)
            query = query.Where(h => h.Difficulte == filter.Difficulte.Value);

        if (filter.PrixMin.HasValue)
            query = query.Where(h => h.Prix >= filter.PrixMin.Value);

        if (filter.PrixMax.HasValue)
            query = query.Where(h => h.Prix <= filter.PrixMax.Value);

        var hikes = await query.OrderBy(h => h.DateDebut).ToListAsync();

        return View(new HikeIndexViewModel { Filter = filter, Hikes = hikes });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var hike = await _context.Hikes
            .AsNoTracking()
            .Include(h => h.Bus)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hike is null)
            return NotFound();

        var acceptedCount = await _context.Participations
            .CountAsync(p => p.HikeId == id && p.Statut == ParticipationStatus.Acceptee);

        var vm = new HikeDetailsViewModel
        {
            Hike = hike,
            AcceptedCount = acceptedCount,
            IsAuthenticated = User.Identity?.IsAuthenticated == true
        };

        if (vm.IsAuthenticated && int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            var participation = await _context.Participations
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.HikeId == id && p.UserId == userId);
            vm.AlreadyRequested = participation != null;
            vm.ExistingParticipationStatus = participation?.Statut.ToString();
            vm.IsFavorite = await _context.Favorites.AnyAsync(f => f.HikeId == id && f.UserId == userId);
        }

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Participate(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Challenge();

        var hike = await _context.Hikes.FirstOrDefaultAsync(h => h.Id == id);
        if (hike is null)
            return NotFound();

        if (hike.DateDebut <= DateTime.Now || hike.Statut == HikeStatus.Annulee || hike.Statut == HikeStatus.Terminee)
        {
            TempData["Error"] = "Cette randonnée n'est pas disponible pour inscription.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (await _context.Participations.AnyAsync(p => p.UserId == userId && p.HikeId == id))
        {
            TempData["Error"] = "Vous avez déjà une demande pour cette randonnée.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var hasOverlap = await _context.Participations
            .Include(p => p.Hike)
            .AnyAsync(p => p.UserId == userId
                && p.Statut == ParticipationStatus.Acceptee
                && p.Hike.DateDebut < hike.DateFin
                && hike.DateDebut < p.Hike.DateFin);

        if (hasOverlap)
        {
            TempData["Error"] = "Conflit de dates avec une randonnée acceptée.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var acceptedCount = await _context.Participations
                .CountAsync(p => p.HikeId == id && p.Statut == ParticipationStatus.Acceptee);

            if (acceptedCount >= hike.Capacite)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Cette randonnée est complète.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Participations.Add(new Participation
            {
                UserId = userId,
                HikeId = id,
                Statut = ParticipationStatus.EnAttente,
                DateCreation = DateTime.UtcNow,
                IsSeenByUser = false
            });

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            TempData["Success"] = "Demande de participation envoyée.";
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Impossible d'enregistrer la demande.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
