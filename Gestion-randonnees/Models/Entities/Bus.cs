namespace Gestion_randonnees.Models.Entities;

public class Bus
{
    public int Id { get; set; }
    public string Immatriculation { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;

    public ICollection<Hike> Hikes { get; set; } = new List<Hike>();
}
