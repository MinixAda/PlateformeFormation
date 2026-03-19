namespace PlateformeFormation.API.Dtos
{
    // DTO utilisé pour l'inscription d'un utilisateur
    public class RegisterDto
    {
        // Email obligatoire
        public string Email { get; set; } = string.Empty;

        // Mot de passe en clair (sera hashé)
        public string Password { get; set; } = string.Empty;

        // Nom de l'utilisateur
        public string Nom { get; set; } = string.Empty;

        // Prénom de l'utilisateur
        public string Prenom { get; set; } = string.Empty;

        // Id du rôle (Admin = 1, Formateur = 2, Apprenant = 3)
        public int RoleId { get; set; }
    }
}
