namespace Gestion_randonnees.Models.Entities;

public class Favorite
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int HikeId { get; set; }
    public Hike Hike { get; set; } = null!;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
