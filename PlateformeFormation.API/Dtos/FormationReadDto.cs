namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé pour renvoyer une formation au client.
    
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
    }
}
