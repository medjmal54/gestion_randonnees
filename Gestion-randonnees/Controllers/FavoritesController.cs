using System.Security.Claims;
using Gestion_randonnees.Data;
using Gestion_randonnees.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly ApplicationDbContext _context;

    public FavoritesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TryGetUserId(out var userId)) return Challenge();

        var favorites = await _context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Include(f => f.Hike)
            .OrderByDescending(f => f.DateCreation)
            .Select(f => f.Hike)
            .ToListAsync();

        return View(favorites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int hikeId)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.HikeId == hikeId);

        if (favorite is null)
        {
            if (!await _context.Hikes.AnyAsync(h => h.Id == hikeId))
                return NotFound();

            _context.Favorites.Add(new Favorite
            {
                UserId = userId,
                HikeId = hikeId,
                DateCreation = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return Json(new { status = "added" });
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();
        return Json(new { status = "removed" });
    }

    private bool TryGetUserId(out int userId)
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
