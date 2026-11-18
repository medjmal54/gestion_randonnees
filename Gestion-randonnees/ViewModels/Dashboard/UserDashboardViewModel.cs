using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Dashboard;

public class UserDashboardViewModel
{
    public IReadOnlyList<Participation> Participations { get; set; } = Array.Empty<Participation>();
    public int Total => Participations.Count;
    public int Accepted => Participations.Count(p => p.Statut == Models.Enums.ParticipationStatus.Acceptee);
    public int Pending => Participations.Count(p => p.Statut == Models.Enums.ParticipationStatus.EnAttente);
    public decimal TotalSpent { get; set; }
}
