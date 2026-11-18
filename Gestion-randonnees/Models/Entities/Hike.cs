using Gestion_randonnees.Models.Enums;

namespace Gestion_randonnees.Models.Entities;

public class Hike
{
    public int Id { get; set; }
    public string Categorie { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string Region { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public Difficulty Difficulte { get; set; }
    public int Capacite { get; set; }
    public string? ImagePath { get; set; }
    public int? BusId { get; set; }
    public Bus? Bus { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public HikeStatus Statut { get; set; } = HikeStatus.Prevue;
    public decimal Prix { get; set; }

    public ICollection<Participation> Participations { get; set; } = new List<Participation>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
