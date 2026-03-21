using System;

namespace PlateformeFormation.Domain.Entities
{
    
    // Représente un module appartenant à une formation.
    // Un module est une unité pédagogique (chapitre, section, etc.).
    
    public class Module
    {
        
        // Identifiant unique du module.
        
        public int Id { get; set; }

        
        // Identifiant de la formation à laquelle ce module appartient.
        
        public int FormationId { get; set; }

        
        // Titre du module (affiché à l'utilisateur).
        
        public string Titre { get; set; } = string.Empty;

        
        // Description détaillée du contenu du module.
        
        public string? Description { get; set; }

        
        // Ordre d'affichage du module dans la formation.
        
        public int Ordre { get; set; }

        
        // Durée estimée du module en minutes.
        
        public int? DureeMinutes { get; set; }
    }
}
