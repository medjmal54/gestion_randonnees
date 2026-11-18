using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Hikes;

public class HikeIndexViewModel
{
    public HikeFilterViewModel Filter { get; set; } = new();
    public IReadOnlyList<Hike> Hikes { get; set; } = Array.Empty<Hike>();
}
