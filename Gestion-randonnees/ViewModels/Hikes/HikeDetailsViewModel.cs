using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Hikes;

public class HikeDetailsViewModel
{
    public Hike Hike { get; set; } = null!;
    public int AcceptedCount { get; set; }
    public bool IsFull => AcceptedCount >= Hike.Capacite;
    public bool IsAuthenticated { get; set; }
    public bool AlreadyRequested { get; set; }
    public bool IsFavorite { get; set; }
    public string? ExistingParticipationStatus { get; set; }
}
