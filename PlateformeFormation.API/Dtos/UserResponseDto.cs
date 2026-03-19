using System;

namespace PlateformeFormation.API.Dtos
{
    // DTO renvoyé au front sans exposer le hash du mot de passe
    // RoleId est renvoyé sous forme d'un int pour simplifier le front
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}
