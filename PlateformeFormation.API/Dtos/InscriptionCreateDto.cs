
// API/Dtos/InscriptionCreateDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/Inscription.</summary>
    public class InscriptionCreateDto
    {
        //ID de la formation à rejoindre.</summary>
        public int FormationId { get; set; }
    }
}