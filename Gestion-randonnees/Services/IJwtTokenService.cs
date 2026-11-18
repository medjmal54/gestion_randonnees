using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.Services;

public interface IJwtTokenService
{
    (string token, DateTime expiresAtUtc) GenerateToken(User user);
}
