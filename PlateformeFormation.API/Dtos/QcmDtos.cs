
// PlateformeFormation.API/Dtos/QcmDtos.cs
//
// DTOs manquants pour le contrôleur QCM.
//
// FICHIERS MANQUANTS IDENTIFIÉS À L'ANALYSE :
//   - QuestionUpdateDto  → référencé dans QcmController.UpdateQuestion()
//                          mais absent du projet original.
//   - ReponseReadDto     → référencé dans QcmController.GetQuestions()
//                          (mapping vers le frontend) mais absent.
//
// Ces deux DTOs DOIVENT exister pour que QcmController compile.


using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    
    // QuestionUpdateDto
    

    //
    // DTO reçu par PUT /api/Qcm/questions/{id}.
    // Permet de modifier le texte et/ou l'ordre d'une question existante.
    // Les réponses ne sont PAS modifiables via ce DTO :
    // pour les modifier, supprimer la question et en créer une nouvelle.
    //DTO reçu par PUT /api/Qcm/questions/{id} (formateur).</summary>
    //
    public class QuestionUpdateDto
    {
        //
        // Nouveau texte de la question.
        // Obligatoire — une question sans texte n'a pas de sens.
        //
        [Required(ErrorMessage = "Le texte de la question est obligatoire.")]
        [MinLength(5, ErrorMessage = "Le texte de la question doit contenir au moins 5 caractères.")]
        [MaxLength(1000, ErrorMessage = "Le texte de la question ne peut pas dépasser 1000 caractères.")]
        public string Texte { get; set; } = string.Empty;

        //
        // Nouvel ordre d'affichage dans le QCM.
        // Si 0 ou non fourni, l'ordre actuel est conservé.
        //
        [Range(0, int.MaxValue, ErrorMessage = "L'ordre doit être un entier positif ou nul.")]
        public int Ordre { get; set; } = 0;
    }

    
    // ReponseReadDto
    

    //
    // DTO renvoyé au frontend pour représenter une réponse possible.
    // IMPORTANT : EstCorrecte est volontairement ABSENT de ce DTO.
    // La réponse correcte ne doit jamais être envoyée au frontend
    // avant la soumission du QCM — la correction se fait côté serveur.
    //
    public class ReponseReadDto
    {
        //Identifiant de la réponse (utilisé dans SoumettreQcmDto.ReponseId).</summary>
        public int Id { get; set; }

        //Texte de la réponse affiché à l'apprenant.</summary>
        public string Texte { get; set; } = string.Empty;

        // EstCorrecte volontairement absent — sécurité anti-triche
    }
}
