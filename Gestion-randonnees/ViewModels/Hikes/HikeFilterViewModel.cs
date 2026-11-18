using Gestion_randonnees.Models.Enums;

namespace Gestion_randonnees.ViewModels.Hikes;

public class HikeFilterViewModel
{
    public string? Region { get; set; }
    public string? Categorie { get; set; }
    public Difficulty? Difficulte { get; set; }
    public decimal? PrixMin { get; set; }
    public decimal? PrixMax { get; set; }
}
