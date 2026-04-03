
// API/Dtos/FormationUpdateDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par PUT /api/Formation/{id}.</summary>
    public class FormationUpdateDto
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

        //Permet de publier ou dépublier une formation.</summary>
        public bool EstPublique { get; set; }
    }
}