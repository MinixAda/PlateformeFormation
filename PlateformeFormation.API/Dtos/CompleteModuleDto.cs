
// API/Dtos/CompleteModuleDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/ModuleProgression/terminer.</summary>
    public class CompleteModuleDto
    {
        //ID du module à marquer comme terminé.</summary>
        public int ModuleId { get; set; }
    }
}