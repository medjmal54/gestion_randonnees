using Gestion_randonnees.Models.Enums;

namespace Gestion_randonnees.Models.Entities;

public class User
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string? Telephone { get; set; }
    public string? ProfileImagePath { get; set; }

    public ICollection<Participation> Participations { get; set; } = new List<Participation>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
