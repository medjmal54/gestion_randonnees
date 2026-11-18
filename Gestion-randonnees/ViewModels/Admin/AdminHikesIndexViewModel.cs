using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Admin;

public class AdminHikesIndexViewModel
{
    public IReadOnlyList<Hike> Hikes { get; set; } = Array.Empty<Hike>();
    public IReadOnlyList<Bus> Buses { get; set; } = Array.Empty<Bus>();
}
