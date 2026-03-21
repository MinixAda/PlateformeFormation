namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé pour créer un utilisateur.
    
    public class UtilisateurCreateDto
    {
        
        // Nom de famille de l'utilisateur.
        
        public string Nom { get; set; } = string.Empty;

        
        // Prénom de l'utilisateur.
        
        public string Prenom { get; set; } = string.Empty;

        
        // Adresse email unique de l'utilisateur.
        
        public string Email { get; set; } = string.Empty;

        
        // Mot de passe en clair (sera hashé avant stockage).
        
        public string Password { get; set; } = string.Empty;

        
        // Identifiant du rôle attribué à l'utilisateur.
        
        public int RoleId { get; set; }
    }
}
