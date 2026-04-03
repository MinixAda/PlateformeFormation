
// API/Dtos/ChangePasswordDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/auth/changer-mot-de-passe.</summary>
    public class ChangePasswordDto
    {
        //Mot de passe actuel pour confirmation (vérifié via BCrypt).</summary>
        public string AncienMotDePasse { get; set; } = string.Empty;

        //Nouveau mot de passe en clair (sera hashé via BCrypt).</summary>
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }
}