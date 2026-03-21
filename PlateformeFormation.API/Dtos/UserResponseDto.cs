namespace PlateformeFormation.API.Dtos
{
    
    // DTO renvoyé au client pour représenter un utilisateur
    // sans exposer d'informations sensibles.
    
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        
        // Identifiant du rôle associé à l'utilisateur.
        
        public int RoleId { get; set; }
    }
}
