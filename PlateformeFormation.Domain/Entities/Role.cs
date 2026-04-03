
// Domain/Entities/Role.cs
//
// Entité représentant un rôle utilisateur dans la plateforme.
// Les trois rôles sont initialisés en base au démarrage via
// RoleRepository.CreateIfNotExistsAsync().


namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente un rôle utilisateur.
    // Valeurs fixes : 1 = Admin, 2 = Formateur, 3 = Apprenant.
    // Correspond à l'enum RoleEnum pour utilisation dans le code.
    //
    public class Role
    {
        //
        // Identifiant unique du rôle.
        // Fixe : 1 = Admin, 2 = Formateur, 3 = Apprenant.
        // Pas de IDENTITY — l'ID est assigné manuellement.
        //
        public int Id { get; set; }

        //Nom du rôle : "Admin", "Formateur" ou "Apprenant".</summary>
        public string Nom { get; set; } = string.Empty;
    }
}