namespace Gestion_randonnees.DTOs;

public record HikeListItemDto(int Id, string Titre, string Region, string Categorie, string Difficulte, decimal Prix, DateTime DateDebut, int Capacite, string? ImagePath);
