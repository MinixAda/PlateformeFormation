namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé lors de la mise à jour d'une formation.
    
    public class FormationUpdateDto
    {
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? MediaType { get; set; }
        public string? ModeDiffusion { get; set; }
        public string? Langue { get; set; }
        public string? Niveau { get; set; }
        public string? Prerequis { get; set; }
        public string? ImageUrl { get; set; }
        public int? DureeMinutes { get; set; }
    }
}
