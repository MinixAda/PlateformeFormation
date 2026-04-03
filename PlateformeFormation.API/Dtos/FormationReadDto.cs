

// DTO utilisé pour renvoyer (mode lecture) une formation au client


// API/Dtos/FormationReadDto.cs

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO renvoyé au client pour représenter une formation.
    // Utilisé par GET /api/Formation et GET /api/Formation/{id}.
    //
    public class FormationReadDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateCreation { get; set; }
        public int CreateurId { get; set; }
        public string? MediaType { get; set; }
        public string? ModeDiffusion { get; set; }
        public string? Langue { get; set; }
        public string? Niveau { get; set; }
        public string? Prerequis { get; set; }
        public string? ImageUrl { get; set; }
        public int? DureeMinutes { get; set; }

        //
        // Visibilité de la formation.
        // Utilisé par le frontend pour afficher un badge "Privée"
        // et filtrer l'affichage selon le rôle.
        //
        public bool EstPublique { get; set; }
    }
}