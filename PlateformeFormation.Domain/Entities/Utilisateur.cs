
// Domain/Entities/Utilisateur.cs
//
// Entité représentant un utilisateur de la plateforme.
//
// CORRECTION APPLIQUÉE :
//   - LienPortfolio ajouté (exigé par les consignes : "Page de profil
//     avec possibilité de lien vers portfolio").
//   - Bio ajoutée pour compléter la page de profil.


namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente un utilisateur inscrit sur la plateforme.
    // Peut avoir le rôle Admin (1), Formateur (2) ou Apprenant (3).
    //
    public class Utilisateur
    {
        //Identifiant unique de l'utilisateur (clé primaire SQL, IDENTITY).</summary>
        public int Id { get; set; }

        //Nom de famille de l'utilisateur.</summary>
        public string Nom { get; set; } = string.Empty;

        //Prénom de l'utilisateur.</summary>
        public string Prenom { get; set; } = string.Empty;

        //
        // Adresse email unique — sert d'identifiant de connexion.
        // Contrainte UNIQUE dans la base SQL.
        //
        public string Email { get; set; } = string.Empty;

        //
        // Hash BCrypt du mot de passe.
        // Le mot de passe en clair n'est jamais stocké — uniquement le hash.
        // Généré via PasswordService.HashPassword().
        //
        public string MotDePasseHash { get; set; } = string.Empty;

        //
        // Identifiant du rôle de l'utilisateur.
        // 1 = Admin, 2 = Formateur, 3 = Apprenant.
        // Clé étrangère vers la table Role.
        //
        public int RoleId { get; set; }

        //
        // Courte biographie ou présentation de l'utilisateur.
        // Affichée sur la page de profil.
        // Nullable — champ optionnel.
        //
        public string? Bio { get; set; }

        //
        // URL vers le portfolio externe de l'utilisateur
        // (GitHub, LinkedIn, site personnel, etc.).
        // Exigé par les consignes TFE : "Page de profil avec possibilité de lien vers portfolio".
        // Nullable — champ optionnel.
        //
        public string? LienPortfolio { get; set; }
    }
}