
// API/Dtos/UtilisateurCreateDto.cs

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO reçu par POST /api/Utilisateurs (admin crée un utilisateur)
    // et PUT /api/Utilisateurs/{id} (admin modifie un utilisateur).
    //
    public class UtilisateurCreateDto
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        //Mot de passe en clair — hashé avant stockage.</summary>
        public string Password { get; set; } = string.Empty;

        //ID du rôle attribué par l'admin (1, 2 ou 3).</summary>
        public int RoleId { get; set; }
    }
}