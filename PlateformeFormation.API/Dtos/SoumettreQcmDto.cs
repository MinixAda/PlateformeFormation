
// PlateformeFormation.API/Dtos/SoumettreQcmDto.cs
//
// DTO UNIQUE pour POST /api/Qcm/{moduleId}/valider.
//
// FUSION de SoumissionQcmDto + SoumettreQcmDto :
//   → SoumissionQcmDto était le DTO original sans validation DataAnnotations.
//   → SoumettreQcmDto ajoute [Required] et [MinLength] pour un retour 400 propre.
//   → Les deux ont été fusionnés ici. SoumissionQcmDto.cs doit être SUPPRIMÉ.
//
// FICHIER À SUPPRIMER : PlateformeFormation.API/Dtos/SoumissionQcmDto.cs


using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO unique pour POST /api/Qcm/{moduleId}/valider.
    // Reçoit la liste des réponses choisies par l'apprenant.
    //
    // Structure attendue (JSON) :
    // {
    //   "reponses": [
    //     { "questionId": 1, "reponseId": 3 },
    //     { "questionId": 2, "reponseId": 7 }
    //   ]
    // }
    //
    public class SoumettreQcmDto
    {
        //
        // Liste des réponses choisies — une par question.
        // Obligatoire et non vide.
        //
        [Required(ErrorMessage = "La liste des réponses est obligatoire.")]
        [MinLength(1, ErrorMessage = "Au moins une réponse est requise.")]
        public List<ReponseChoisieQcmDto> Reponses { get; set; } = new();
    }

    //
    // Association question → réponse choisie par l'apprenant.
    // Fusion de l'ancien ReponseChoisieDto (sans validation)
    // et du nouveau ReponseChoisieQcmDto (avec [Required]).
    //
    public class ReponseChoisieQcmDto
    {
        //ID de la question à laquelle l'apprenant répond.</summary>
        [Required(ErrorMessage = "L'identifiant de la question est obligatoire.")]
        public int QuestionId { get; set; }

        //ID de la réponse choisie par l'apprenant.</summary>
        [Required(ErrorMessage = "L'identifiant de la réponse est obligatoire.")]
        public int ReponseId { get; set; }
    }

    //
    // Résultat renvoyé après correction côté serveur.
    // Contient le score, le pourcentage et un indicateur si la formation
    // est entièrement terminée suite à ce QCM.
    //
    public class ResultatQcmCompletDto
    {
        //Nombre de bonnes réponses.</summary>
        public int Score { get; set; }

        //Nombre total de questions.</summary>
        public int Total { get; set; }

        //true si Score / Total >= 60%.</summary>
        public bool Reussi { get; set; }

        //Pourcentage de bonnes réponses (arrondi à l'entier).</summary>
        public int Pourcentage { get; set; }

        //Message explicite renvoyé au frontend.</summary>
        public string Message { get; set; } = string.Empty;

        //
        // true si c'était le dernier module et que la formation est
        // maintenant complètement terminée.
        // Permet au frontend d'afficher "Votre attestation est disponible".
        //
        public bool FormationTerminee { get; set; }
    }
}
