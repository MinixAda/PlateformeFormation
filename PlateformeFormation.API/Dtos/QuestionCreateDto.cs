
// API/Dtos/QuestionCreateDto.cs

using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    //DTO reçu par POST /api/Qcm/{moduleId}/questions (formateur).</summary>
    public class QuestionCreateDto
    {
        [Required(ErrorMessage = "Le texte de la question est obligatoire.")]
        public string Texte { get; set; } = string.Empty;

        public int Ordre { get; set; }

        //Réponses à créer avec la question (au moins 2 requises).</summary>
        public List<ReponseCreateDto> Reponses { get; set; } = new();
    }

    //DTO pour créer une réponse avec sa question parente.</summary>
    public class ReponseCreateDto
    {
        [Required(ErrorMessage = "Le texte de la réponse est obligatoire.")]
        public string Texte { get; set; } = string.Empty;

        //true = bonne réponse. Une seule réponse par question doit être correcte.</summary>
        public bool EstCorrecte { get; set; }
    }

    /*//DTO reçu par PUT /api/Qcm/questions/{id} (formateur).</summary>
    public class QuestionUpdateDto
    {
        [Required(ErrorMessage = "Le texte de la question est obligatoire.")]
        public string Texte { get; set; } = string.Empty;
        public int Ordre { get; set; }
    }*/
}