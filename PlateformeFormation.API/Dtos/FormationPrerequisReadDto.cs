
// DTO utilisé pour renvoyer un prérequis au client


// API/Dtos/FormationPrerequisReadDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO renvoyé par GET /api/FormationPrerequis/{formationId}.</summary>
    public class FormationPrerequisReadDto
    {
        public int FormationId { get; set; }
        public int FormationRequiseId { get; set; }
    }
}


