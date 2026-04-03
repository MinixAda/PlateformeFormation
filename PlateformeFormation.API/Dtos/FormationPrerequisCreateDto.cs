
// API/Dtos/FormationPrerequisCreateDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/FormationPrerequis/{formationId}.</summary>
    public class FormationPrerequisCreateDto
    {
        //ID de la formation qui doit être terminée en prérequis.</summary>
        public int FormationRequiseId { get; set; }
    }
}