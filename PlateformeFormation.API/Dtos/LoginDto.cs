
// API/Dtos/LoginDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/auth/login.</summary>
    public class LoginDto
    {
        //Adresse email de l'utilisateur.</summary>
        public string Email { get; set; } = string.Empty;

        //Mot de passe en clair (sera vérifié via BCrypt).</summary>
        public string MotDePasse { get; set; } = string.Empty;
    }
}