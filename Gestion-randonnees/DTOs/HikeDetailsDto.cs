namespace Gestion_randonnees.DTOs;

public record HikeDetailsDto(
    int Id,
    string Titre,
    string Categorie,
    string Description,
    DateTime DateDebut,
    DateTime DateFin,
    string Region,
    double DistanceKm,
    string Difficulte,
    int Capacite,
    int AcceptedCount,
    string? ImagePath,
    decimal Prix,
    string Statut,
    string? BusImmatriculation);
