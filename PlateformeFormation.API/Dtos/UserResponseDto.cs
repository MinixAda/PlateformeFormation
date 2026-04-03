
// API/Dtos/UserResponseDto.cs
//
// CORRECTIONS APPLIQUÉES :
//   - RoleNom ajouté (évite un appel API supplémentaire pour afficher le rôle)
//   - Bio ajouté (page de profil)
//   - LienPortfolio ajouté (exigé par les consignes TFE)

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO renvoyé au client pour représenter un utilisateur.
    // Utilisé par GET /api/auth/me, GET /api/Utilisateurs, GET /api/Utilisateurs/{id}.
    // NE contient JAMAIS le hash du mot de passe.
    //
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }

        //Nom lisible du rôle — évite un GET /api/Role supplémentaire.</summary>
        public string RoleNom { get; set; } = string.Empty;

        //Courte biographie de l'utilisateur (page de profil).</summary>
        public string? Bio { get; set; }

        //
        // Lien vers le portfolio externe.
        // Exigé par les consignes : "Page de profil avec possibilité de lien vers portfolio".
        //
        public string? LienPortfolio { get; set; }
    }
}