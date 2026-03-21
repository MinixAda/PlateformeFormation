using System;

namespace PlateformeFormation.API.Dtos.Progression
{
    
    // DTO représentant la progression sur un module.
    
    public class ModuleProgressionReadDto
    {
        public int ModuleId { get; set; }
        public bool EstTermine { get; set; }
        public DateTime DateCompletion { get; set; }
    }
}
