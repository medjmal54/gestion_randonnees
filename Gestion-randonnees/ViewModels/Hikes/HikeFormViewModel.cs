using System.ComponentModel.DataAnnotations;
using Gestion_randonnees.Models.Enums;

namespace Gestion_randonnees.ViewModels.Hikes;

public class HikeFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(150)]
    public string Titre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Categorie { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime DateDebut { get; set; }

    [Required]
    public DateTime DateFin { get; set; }

    [Required, StringLength(100)]
    public string Region { get; set; } = string.Empty;

    [Range(0.1, 10000)]
    public double DistanceKm { get; set; }

    [Required]
    public Difficulty Difficulte { get; set; }

    [Range(1, 10000)]
    public int Capacite { get; set; }

    [Range(0, 1000000)]
    public decimal Prix { get; set; }

    public HikeStatus Statut { get; set; } = HikeStatus.Prevue;

    public int? BusId { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? ExistingImagePath { get; set; }
}
