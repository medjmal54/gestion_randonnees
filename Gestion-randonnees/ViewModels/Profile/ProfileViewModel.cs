using System.ComponentModel.DataAnnotations;

namespace Gestion_randonnees.ViewModels.Profile;

public class ProfileViewModel
{
    [Required, StringLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telephone { get; set; }

    public string? ProfileImagePath { get; set; }
    public IFormFile? Photo { get; set; }
}
