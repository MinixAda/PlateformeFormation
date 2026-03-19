namespace PlateformeFormation.Domain.Entities
{
    
    // Entité représentant un utilisateur dans la base SQL.

    public class Utilisateur
    {
        public int Id { get; set; }   // PK
        public string Nom { get; set; } = string.Empty; // Nom
        public string Prenom { get; set; } = string.Empty; // Prénom
        public string Email { get; set; } = string.Empty;  // Email unique
        public string MotDePasseHash { get; set; } = string.Empty; // Hash BCrypt

        /*  RoleId doit être un INT (pas un enum)
         car la base SQL stocke un int*/

        public int RoleId { get; set; }   // FK vers Role
    }
}
