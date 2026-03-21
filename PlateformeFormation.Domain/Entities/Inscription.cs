namespace PlateformeFormation.Domain.Entities
{
    
    // Représente l'inscription d'un utilisateur à une formation.
    
    public class Inscription
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public int FormationId { get; set; }
        public DateTime DateInscription { get; set; }

        
        // Statut de l'inscription (EnCours, Validee, Refusee...).
        
        public string Statut { get; set; } = "EnCours";
    }
}
