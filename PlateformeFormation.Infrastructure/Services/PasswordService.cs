
// Infrastructure/Services/PasswordService.cs
//
// Service responsable du hachage et de la vérification des mots de passe.
// Utilise BCrypt (algorithme sécurisé avec sel automatique).


using BCrypt.Net;

namespace PlateformeFormation.Infrastructure.Services
{
    //
    // Service de gestion des mots de passe via l'algorithme BCrypt.
    // Enregistré comme Singleton dans Program.cs car il est sans état.
    //
    // BCrypt génère automatiquement un sel unique pour chaque hash,
    // rendant les attaques par table arc-en-ciel inefficaces.
    // Work factor par défaut : 11 (bon équilibre sécurité / performance).
    //
    public class PasswordService
    {
        //
        // Génère un hash BCrypt sécurisé à partir d'un mot de passe en clair.
        // Le sel est inclus dans le hash retourné — pas besoin de le stocker séparément.
        //
        // À appeler lors de la création d'un compte ou du changement de mot de passe.
        // NE JAMAIS stocker le mot de passe en clair.
        //
        // <param name="password">Mot de passe en clair fourni par l'utilisateur.</param>
        // <returns>Hash BCrypt à stocker dans la colonne MotDePasseHash.</returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        //
        // Vérifie qu'un mot de passe en clair correspond à un hash BCrypt stocké.
        // BCrypt extrait le sel depuis le hash pour effectuer la comparaison.
        //
        // À appeler lors de la connexion (POST /api/auth/login)
        // et du changement de mot de passe (POST /api/auth/changer-mot-de-passe).
        //
        // <param name="password">Mot de passe en clair saisi par l'utilisateur.</param>
        // <param name="hash">Hash BCrypt stocké en base pour cet utilisateur.</param>
        // <returns>true si le mot de passe correspond au hash, false sinon.</returns>
        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}