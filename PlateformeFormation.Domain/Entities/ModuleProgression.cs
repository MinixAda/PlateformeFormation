using System;

namespace PlateformeFormation.Domain.Entities
{
    
    // Représente la progression d'un utilisateur sur un module.
    // Une ligne = un module terminé par un utilisateur.
    
    public class ModuleProgression
    {
        
        // Identifiant unique de la progression.
        
        public int Id { get; set; }

        
        // Identifiant de l'utilisateur.
        
        public int UtilisateurId { get; set; }

        
        // Identifiant du module terminé.
        
        public int ModuleId { get; set; }

        
        // Date de complétion du module.
        
        public DateTime DateCompletion { get; set; }

        
        // Indique si le module est terminé (toujours true ici).
        
        public bool EstTermine { get; set; }
    }
}
