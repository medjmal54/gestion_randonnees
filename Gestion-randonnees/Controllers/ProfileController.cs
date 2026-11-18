using System.Security.Claims;
using Gestion_randonnees.Data;
using Gestion_randonnees.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gestion_randonnees.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProfileController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return Challenge();

        return View(new ProfilePageViewModel
        {
            Profile = new ProfileViewModel
            {
                Nom = user.Nom,
                Email = user.Email,
                Telephone = user.Telephone,
                ProfileImagePath = user.ProfileImagePath
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
        {
            return View("Index", new ProfilePageViewModel { Profile = model });
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var emailTaken = await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId);
        if (emailTaken)
        {
            ModelState.AddModelError("Profile.Email", "Cet email est déjà utilisé.");
            return View("Index", new ProfilePageViewModel { Profile = model });
        }

        user.Nom = model.Nom.Trim();
        user.Email = email;
        user.Telephone = model.Telephone?.Trim();
        await _context.SaveChangesAsync();
        TempData["Success"] = "Profil mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ProfilePageViewModel pageModel)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return Challenge();

        var model = pageModel.ChangePassword;
        if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
        {
            TempData["Error"] = "Veuillez remplir tous les champs du mot de passe.";
            return RedirectToAction(nameof(Index));
        }

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
        {
            TempData["Error"] = "Mot de passe actuel incorrect.";
            return RedirectToAction(nameof(Index));
        }

        if (model.NewPassword != model.ConfirmNewPassword)
        {
            TempData["Error"] = "La confirmation ne correspond pas.";
            return RedirectToAction(nameof(Index));
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Mot de passe modifié.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePhoto(IFormFile? photo)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return Challenge();

        if (photo is null || photo.Length == 0)
        {
            TempData["Error"] = "Aucune image sélectionnée.";
            return RedirectToAction(nameof(Index));
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext) || photo.Length > 2 * 1024 * 1024)
        {
            TempData["Error"] = "Image invalide (jpg/png/webp, max 2 Mo).";
            return RedirectToAction(nameof(Index));
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"user_{userId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await photo.CopyToAsync(stream);
        }

        user.ProfileImagePath = $"/uploads/profiles/{fileName}";
        await _context.SaveChangesAsync();
        TempData["Success"] = "Photo mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    private bool TryGetUserId(out int userId)
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
