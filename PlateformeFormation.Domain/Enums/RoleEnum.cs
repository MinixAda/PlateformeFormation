
// Domain/Enums/RoleEnum.cs
//
// Enumération des rôles disponibles sur la plateforme.
// Les valeurs correspondent aux IDs fixes dans la table Role SQL.


namespace PlateformeFormation.Domain.Enums
{
    //
    // Rôles possibles pour un utilisateur de la plateforme.
    // Ces valeurs doivent correspondre aux IDs dans la table Role :
    //   INSERT INTO Role (Id, Nom) VALUES (1,'Admin'), (2,'Formateur'), (3,'Apprenant')
    //
    // Utilisation dans le code :
    //   if (user.RoleId == (int)RoleEnum.Admin) { ... }
    //   [Authorize(Roles = "1,2")]  // Admin + Formateur
    //
    public enum RoleEnum
    {
        //Administrateur de la plateforme — accès complet à toutes les fonctionnalités.</summary>
        Admin = 1,

        //Formateur — peut créer et gérer ses formations et modules.</summary>
        Formateur = 2,

        //Apprenant — peut s'inscrire aux formations et suivre sa progression.</summary>
        Apprenant = 3
    }
}