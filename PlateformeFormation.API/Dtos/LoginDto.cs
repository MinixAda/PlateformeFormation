namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé pour la connexion (login).
    
    public class LoginDto
    {
        
        // Email de l'utilisateur.
        
        public string Email { get; set; } = string.Empty;

        
        // Mot de passe en clair saisi par l'utilisateur.
        // Il sera vérifié par rapport au hash stocké.
        
        public string MotDePasse { get; set; } = string.Empty;
    }
}
