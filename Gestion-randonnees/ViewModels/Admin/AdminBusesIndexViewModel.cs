using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Admin;

public class AdminBusesIndexViewModel
{
    public IReadOnlyList<Bus> Buses { get; set; } = Array.Empty<Bus>();
}
