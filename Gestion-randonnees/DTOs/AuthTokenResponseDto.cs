namespace Gestion_randonnees.DTOs;

public record AuthTokenResponseDto(string AccessToken, DateTime ExpiresAtUtc, string TokenType = "Bearer");
