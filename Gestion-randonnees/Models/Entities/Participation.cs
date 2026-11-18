using Gestion_randonnees.Models.Enums;

namespace Gestion_randonnees.Models.Entities;

public class Participation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int HikeId { get; set; }
    public Hike Hike { get; set; } = null!;
    public ParticipationStatus Statut { get; set; } = ParticipationStatus.EnAttente;
    public bool IsSeenByUser { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
