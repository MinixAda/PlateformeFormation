using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé lors de la création d'un module.
    
    public class ModuleCreateDto
    {
        [Required]
        public string Titre { get; set; } = string.Empty;

        public string? Description { get; set; }

        
        // Ordre d'affichage du module dans la formation.
        
        public int Ordre { get; set; }

        
        // Durée estimée du module en minutes.
        
        public int? DureeMinutes { get; set; }
    }
}
