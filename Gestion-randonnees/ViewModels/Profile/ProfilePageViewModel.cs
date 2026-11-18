using Gestion_randonnees.Models.Entities;

namespace Gestion_randonnees.ViewModels.Profile;

public class ProfilePageViewModel
{
    public ProfileViewModel Profile { get; set; } = new();
    public ChangePasswordViewModel ChangePassword { get; set; } = new();
}
