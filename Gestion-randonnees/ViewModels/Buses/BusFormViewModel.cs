using System.ComponentModel.DataAnnotations;

namespace Gestion_randonnees.ViewModels.Buses;

public class BusFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(50)]
    public string Immatriculation { get; set; } = string.Empty;

    [Range(1, 500)]
    public int SeatCount { get; set; }

    [Required, StringLength(100)]
    public string DriverName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string DriverPhone { get; set; } = string.Empty;
}
