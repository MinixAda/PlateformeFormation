
using System;
namespace PlateformeFormation.API.Dtos
{
  
    // DTO utilisé pour la connexion.
  
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;     // Email de connexion
        public string Password { get; set; } = string.Empty;  // Mot de passe en clair
    }
}
