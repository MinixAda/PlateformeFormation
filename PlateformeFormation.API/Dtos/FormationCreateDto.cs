namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé lors de la création d'une formation
    
    public class FormationCreateDto
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

        // Visibilité — bool false par défaut (Formation en mode publication
        // privée jusqu'à publication)
        public bool EstPublique { get; set; } = false;

    }
}
