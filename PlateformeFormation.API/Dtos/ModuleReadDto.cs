namespace PlateformeFormation.API.Dtos.Module
{
    
    // DTO renvoyé pour un module d'une formation.
    
    public class ModuleReadDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Ordre { get; set; }
        public int? DureeMinutes { get; set; }
    }
}
