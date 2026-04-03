
// API/Dtos/RegisterDto.cs
//
// CORRECTION APPLIQUÉE :
//   RoleId supprimé — le rôle est forcé à 3 (Apprenant) côté serveur.
//   L'ancienne version avait un RoleId int non-nullable qui provoquait
//   une valeur 0 si le frontend ne l'envoyait pas, causant une erreur 500.

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO reçu par POST /api/auth/register.
    // Le rôle n'est PAS inclus — il est forcé à "Apprenant" (Id=3)
    // côté serveur pour empêcher l'auto-attribution de droits élevés.
    //
    public class RegisterDto
    {
        //Nom de famille.</summary>
        public string Nom { get; set; } = string.Empty;

        //Prénom.</summary>
        public string Prenom { get; set; } = string.Empty;

        //Adresse email unique — servira d'identifiant de connexion.</summary>
        public string Email { get; set; } = string.Empty;

        //Mot de passe en clair (sera hashé via BCrypt côté serveur).</summary>
        public string MotDePasse { get; set; } = string.Empty;
    }
}