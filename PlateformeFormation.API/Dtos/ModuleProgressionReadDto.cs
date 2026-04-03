

// API/Dtos/ModuleProgressionReadDto.cs

namespace PlateformeFormation.API.Dtos
{
    // DTO représentant la progression sur un module

    // DTO renvoyé par GET /api/ModuleProgression/formation/{id}.
    // Utilisé par ProgressionPage et AttestationPage côté frontend.
    //
    public class ModuleProgressionReadDto
    {
        public int ModuleId { get; set; }
        public bool EstTermine { get; set; }

        //
        // Date de complétion du module.
        // Null si le module n'est pas encore terminé
        // (ce cas ne se produit que si on retourne tous les modules,
        // pas seulement les terminés — prévu pour une évolution future).
        //
        public DateTime? DateCompletion { get; set; }
    }
}