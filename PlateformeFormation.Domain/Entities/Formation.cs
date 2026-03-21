using System;

namespace PlateformeFormation.Domain.Entities
{
    
    // Représente une formation créée par un formateur.
    // Contient toutes les informations nécessaires pour l'affichage et la gestion.
    
    public class Formation
    {
        public int Id { get; set; }

        
        // Titre de la formation (obligatoire)
        
        public string Titre { get; set; } = string.Empty;

        
        // Description détaillée de la formation
        
        public string? Description { get; set; }

        
        // Date de création de la formation
        
        public DateTime DateCreation { get; set; }

        
        // ID du formateur ayant créé la formation
        
        public int CreateurId { get; set; }

     

        
        // Type de média principal : video, pdf, tp, qcm, etc.
        
        public string? MediaType { get; set; }

        
        // Mode de diffusion : presentiel, distanciel, hybride
        
        public string? ModeDiffusion { get; set; }

        
        // Langue de la formation : FR, EN, NL...
        
        public string? Langue { get; set; }

        
        // Niveau de difficulté : Débutant, Intermédiaire, Avancé
        
        public string? Niveau { get; set; }

        
        // Prérequis nécessaires pour suivre la formation
        
        public string? Prerequis { get; set; }

        
        // URL d'une image d'illustration
        
        public string? ImageUrl { get; set; }

        
        // Durée totale estimée de la formation (en minutes)
        
        public int? DureeMinutes { get; set; }
    }
}
