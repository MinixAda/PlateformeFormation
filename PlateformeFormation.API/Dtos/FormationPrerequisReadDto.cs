namespace PlateformeFormation.API.Dtos.Prerequis
{
    
    // DTO utilisé pour renvoyer un prérequis au client.
    
    public class FormationPrerequisReadDto
    {
        public int FormationId { get; set; }
        public int FormationRequiseId { get; set; }
    }
}
