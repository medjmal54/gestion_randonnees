using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Participations;

public class AdminParticipationsViewModel
{
    public IReadOnlyList<Participation> Pending { get; set; } = Array.Empty<Participation>();
    public IReadOnlyList<Participation> Treated { get; set; } = Array.Empty<Participation>();
}
