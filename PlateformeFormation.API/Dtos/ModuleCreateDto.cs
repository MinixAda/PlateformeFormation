
// API/Dtos/ModuleCreateDto.cs

using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/Formation/{id}/modules.</summary>
    public class ModuleCreateDto
    {
        //Titre obligatoire du module.</summary>
        [Required(ErrorMessage = "Le titre du module est obligatoire.")]
        public string Titre { get; set; } = string.Empty;

        //Description du contenu du module (optionnel).</summary>
        public string? Description { get; set; }

        //Ordre d'affichage dans la formation (1 = premier).</summary>
        public int Ordre { get; set; }

        //Durée estimée en minutes (optionnel).</summary>
        public int? DureeMinutes { get; set; }
    }
}