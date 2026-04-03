

// API/Dtos/ModuleReadDto.cs

namespace PlateformeFormation.API.Dtos
{
    // DTO renvoyé au client pour représenter un module, lorsqu'un module est récupéré.
    // Utilisé dans les endpoints GET (/api/Formation/{id}/modules) des modules d'une formation.
   
    public class ModuleReadDto
    {
        // Identifiant unique du module
        public int Id { get; set; }

            // Id Formation
        public int FormationId { get; set; }

        // Titre du module.
        public string Titre { get; set; } = string.Empty;

        // Description du contenu du module.
        public string? Description { get; set; }

        // Ordre d'affichage du module dans la formation.
        public int Ordre { get; set; }

        // Durée estimée du module en minutes.
        // Peut être null si non renseignée.    
        public int? DureeMinutes { get; set; }
    }
}