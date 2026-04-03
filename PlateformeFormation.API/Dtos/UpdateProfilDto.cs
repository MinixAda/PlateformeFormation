
// API/Dtos/UpdateProfilDto.cs
//
// NOUVEAU DTO — permet à un utilisateur de mettre à jour
// ses informations de profil depuis ProfilPage.
// Exigé par les consignes : "Page de profil avec possibilité
// de lien vers portfolio".

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO reçu par PATCH /api/auth/profil.
    // Seuls Bio et LienPortfolio sont modifiables par l'utilisateur lui-même.
    // Nom, Email et RoleId restent sous contrôle admin.
    //
    public class UpdateProfilDto
    {
        //Courte biographie (optionnel, peut être null pour effacer).</summary>
        public string? Bio { get; set; }

        //
        // URL vers le portfolio externe.
        // Exemples : https://github.com/user, https://linkedin.com/in/user.
        // Nullable — optionnel.
        //
        public string? LienPortfolio { get; set; }
    }
}