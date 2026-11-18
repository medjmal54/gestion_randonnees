using Gestion_randonnees.Data;
using Gestion_randonnees.Models.Enums;
using Gestion_randonnees.ViewModels.Admin;
using Gestion_randonnees.ViewModels.Buses;
using Gestion_randonnees.ViewModels.Dashboard;
using Gestion_randonnees.ViewModels.Hikes;
using Gestion_randonnees.ViewModels.Participations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(DateTime? dateFrom, DateTime? dateTo)
    {
        var to = (dateTo ?? DateTime.Today).Date;
        var from = (dateFrom ?? to.AddDays(-30)).Date;
        var toEnd = to.AddDays(1);

        var participations = await _context.Participations
            .AsNoTracking()
            .Include(p => p.Hike)
            .Where(p => p.DateCreation >= from && p.DateCreation < toEnd)
            .ToListAsync();

        var vm = new AdminDashboardViewModel
        {
            DateFrom = from,
            DateTo = to,
            TotalHikes = await _context.Hikes.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(u => u.Role == UserRole.User),
            TotalRequests = participations.Count,
            AcceptedRequests = participations.Count(p => p.Statut == ParticipationStatus.Acceptee),
            RefusedRequests = participations.Count(p => p.Statut == ParticipationStatus.Refusee),
            PendingRequests = participations.Count(p => p.Statut == ParticipationStatus.EnAttente),
            Revenue = participations.Where(p => p.Statut == ParticipationStatus.Acceptee).Sum(p => p.Hike?.Prix ?? 0m)
        };

        return View(vm);
    }

    [HttpGet("Hikes")]
    public async Task<IActionResult> Hikes()
    {
        var vm = new AdminHikesIndexViewModel
        {
            Hikes = await _context.Hikes.AsNoTracking().Include(h => h.Bus).OrderByDescending(h => h.DateCreation).ToListAsync(),
            Buses = await _context.Buses.AsNoTracking().OrderBy(b => b.Immatriculation).ToListAsync()
        };
        return View(vm);
    }

    [HttpGet("Hikes/Create")]
    public async Task<IActionResult> CreateHike()
    {
        ViewBag.Buses = await _context.Buses.AsNoTracking().OrderBy(b => b.Immatriculation).ToListAsync();
        return View("HikeForm", new HikeFormViewModel { DateDebut = DateTime.Now.Date.AddDays(1).AddHours(8), DateFin = DateTime.Now.Date.AddDays(1).AddHours(18) });
    }

    [HttpPost("Hikes/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateHike(HikeFormViewModel model)
    {
        if (model.DateFin <= model.DateDebut)
            ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être après la date de début.");

        if (!ModelState.IsValid)
        {
            ViewBag.Buses = await _context.Buses.AsNoTracking().ToListAsync();
            return View("HikeForm", model);
        }

        var hike = new Models.Entities.Hike
        {
            Titre = model.Titre.Trim(),
            Categorie = model.Categorie.Trim(),
            Description = model.Description.Trim(),
            DateDebut = model.DateDebut,
            DateFin = model.DateFin,
            Region = model.Region.Trim(),
            DistanceKm = model.DistanceKm,
            Difficulte = model.Difficulte,
            Capacite = model.Capacite,
            Prix = model.Prix,
            Statut = model.Statut,
            BusId = model.BusId,
            DateCreation = DateTime.UtcNow
        };

        hike.ImagePath = await SaveImageAsync(model.ImageFile, "randonnees");
        _context.Hikes.Add(hike);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Hikes));
    }

    [HttpGet("Hikes/Edit/{id:int}")]
    public async Task<IActionResult> EditHike(int id)
    {
        var hike = await _context.Hikes.FindAsync(id);
        if (hike is null) return NotFound();
        ViewBag.Buses = await _context.Buses.AsNoTracking().ToListAsync();
        return View("HikeForm", new HikeFormViewModel
        {
            Id = hike.Id,
            Titre = hike.Titre,
            Categorie = hike.Categorie,
            Description = hike.Description,
            DateDebut = hike.DateDebut,
            DateFin = hike.DateFin,
            Region = hike.Region,
            DistanceKm = hike.DistanceKm,
            Difficulte = hike.Difficulte,
            Capacite = hike.Capacite,
            Prix = hike.Prix,
            Statut = hike.Statut,
            BusId = hike.BusId,
            ExistingImagePath = hike.ImagePath
        });
    }

    [HttpPost("Hikes/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditHike(int id, HikeFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var hike = await _context.Hikes.FindAsync(id);
        if (hike is null) return NotFound();
        if (model.DateFin <= model.DateDebut)
            ModelState.AddModelError(nameof(model.DateFin), "La date de fin doit être après la date de début.");

        if (!ModelState.IsValid)
        {
            ViewBag.Buses = await _context.Buses.AsNoTracking().ToListAsync();
            model.ExistingImagePath = hike.ImagePath;
            return View("HikeForm", model);
        }

        hike.Titre = model.Titre.Trim();
        hike.Categorie = model.Categorie.Trim();
        hike.Description = model.Description.Trim();
        hike.DateDebut = model.DateDebut;
        hike.DateFin = model.DateFin;
        hike.Region = model.Region.Trim();
        hike.DistanceKm = model.DistanceKm;
        hike.Difficulte = model.Difficulte;
        hike.Capacite = model.Capacite;
        hike.Prix = model.Prix;
        hike.Statut = model.Statut;
        hike.BusId = model.BusId;

        var newImage = await SaveImageAsync(model.ImageFile, "randonnees");
        if (!string.IsNullOrEmpty(newImage))
            hike.ImagePath = newImage;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Hikes));
    }

    [HttpPost("Hikes/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHike(int id)
    {
        var hike = await _context.Hikes.FindAsync(id);
        if (hike is null) return NotFound();
        _context.Hikes.Remove(hike);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Hikes));
    }

    [HttpGet("Participations")]
    public async Task<IActionResult> Participations()
    {
        var all = await _context.Participations
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Hike)
            .OrderByDescending(p => p.DateCreation)
            .ToListAsync();

        return View(new AdminParticipationsViewModel
        {
            Pending = all.Where(p => p.Statut == ParticipationStatus.EnAttente).ToList(),
            Treated = all.Where(p => p.Statut != ParticipationStatus.EnAttente).ToList()
        });
    }

    [HttpPost("Participations/{id:int}/Status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateParticipationStatus(int id, ParticipationStatus status)
    {
        if (status == ParticipationStatus.EnAttente) return BadRequest();
        var participation = await _context.Participations.Include(p => p.Hike).FirstOrDefaultAsync(p => p.Id == id);
        if (participation is null) return NotFound();

        if (status == ParticipationStatus.Acceptee)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            var acceptedCount = await _context.Participations
                .CountAsync(p => p.HikeId == participation.HikeId && p.Statut == ParticipationStatus.Acceptee && p.Id != id);

            if (participation.Hike is null || acceptedCount >= participation.Hike.Capacite)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Capacité atteinte, impossible d'accepter.";
                return RedirectToAction(nameof(Participations));
            }

            var hasOverlap = await _context.Participations
                .Include(p => p.Hike)
                .AnyAsync(p => p.UserId == participation.UserId
                    && p.Id != id
                    && p.Statut == ParticipationStatus.Acceptee
                    && p.Hike.DateDebut < participation.Hike.DateFin
                    && participation.Hike.DateDebut < p.Hike.DateFin);
            if (hasOverlap)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Conflit de dates avec une autre randonnée acceptée.";
                return RedirectToAction(nameof(Participations));
            }

            participation.Statut = status;
            participation.IsSeenByUser = false;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        else
        {
            participation.Statut = status;
            participation.IsSeenByUser = false;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Participations));
    }

    [HttpGet("Buses")]
    public async Task<IActionResult> Buses()
    {
        return View(new AdminBusesIndexViewModel { Buses = await _context.Buses.AsNoTracking().OrderBy(b => b.Immatriculation).ToListAsync() });
    }

    [HttpGet("Buses/Create")]
    public IActionResult CreateBus() => View("BusForm", new BusFormViewModel());

    [HttpPost("Buses/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBus(BusFormViewModel model)
    {
        if (!ModelState.IsValid) return View("BusForm", model);
        _context.Buses.Add(new Models.Entities.Bus
        {
            Immatriculation = model.Immatriculation.Trim(),
            SeatCount = model.SeatCount,
            DriverName = model.DriverName.Trim(),
            DriverPhone = model.DriverPhone.Trim()
        });
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Buses));
    }

    [HttpGet("Buses/Edit/{id:int}")]
    public async Task<IActionResult> EditBus(int id)
    {
        var bus = await _context.Buses.FindAsync(id);
        if (bus is null) return NotFound();
        return View("BusForm", new BusFormViewModel { Id = bus.Id, Immatriculation = bus.Immatriculation, SeatCount = bus.SeatCount, DriverName = bus.DriverName, DriverPhone = bus.DriverPhone });
    }

    [HttpPost("Buses/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBus(int id, BusFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var bus = await _context.Buses.FindAsync(id);
        if (bus is null) return NotFound();
        if (!ModelState.IsValid) return View("BusForm", model);

        bus.Immatriculation = model.Immatriculation.Trim();
        bus.SeatCount = model.SeatCount;
        bus.DriverName = model.DriverName.Trim();
        bus.DriverPhone = model.DriverPhone.Trim();
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Buses));
    }

    [HttpPost("Buses/Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBus(int id)
    {
        var bus = await _context.Buses.FindAsync(id);
        if (bus is null) return NotFound();
        _context.Buses.Remove(bus);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Buses));
    }

    private async Task<string?> SaveImageAsync(IFormFile? file, string folder)
    {
        if (file is null || file.Length == 0) return null;
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext) || file.Length > 5 * 1024 * 1024) return null;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(uploadsDir, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{fileName}";
    }
}
