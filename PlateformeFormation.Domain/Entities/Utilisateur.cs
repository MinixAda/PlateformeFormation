namespace PlateformeFormation.Domain.Entities
{
    
    // Représente un utilisateur de la plateforme.
    
    public class Utilisateur
    {
        
        // Identifiant unique de l'utilisateur.
        
        public int Id { get; set; }

        
        // Nom de famille de l'utilisateur.
        
        public string Nom { get; set; } = string.Empty;

        
        // Prénom de l'utilisateur.
        
        public string Prenom { get; set; } = string.Empty;

        
        // Adresse email unique.
        
        public string Email { get; set; } = string.Empty;

        
        // Hash BCrypt du mot de passe.
        
        public string MotDePasseHash { get; set; } = string.Empty;

        
        // Identifiant du rôle associé à l'utilisateur.
        
        public int RoleId { get; set; }
    }
}
