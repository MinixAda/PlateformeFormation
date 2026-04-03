
// PlateformeFormation.API/Dtos/QuestionReadDto.cs — VERSION CORRIGÉE
//
// Ce fichier existait mais référençait ReponseReadDto qui était ABSENT.
// Avec la création de QcmDtos.cs (ReponseReadDto + QuestionUpdateDto),
// ce fichier compile désormais correctement.
//
// VÉRIFICATION : le fichier original contenait déjà QuestionReadDto
// mais SANS la liste de réponses. Cette version corrigée l'inclut.


using System.Collections.Generic;

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO renvoyé au frontend pour représenter une question avec ses réponses.
    // Utilisé par GET /api/Qcm/{moduleId}/questions.
    //
    // SÉCURITÉ : ne contient PAS EstCorrecte dans les réponses.
    // La bonne réponse n'est jamais envoyée au frontend avant soumission.
    //
    public class QuestionReadDto
    {
        //Identifiant de la question.</summary>
        public int Id { get; set; }

        //Ordre d'affichage dans le QCM.</summary>
        public int Ordre { get; set; }

        //Texte de la question affiché à l'apprenant.</summary>
        public string Texte { get; set; } = string.Empty;

        //
        // Liste des réponses possibles.
        // EstCorrecte est absent de ReponseReadDto — sécurité anti-triche.
        //
        public List<ReponseReadDto> Reponses { get; set; } = new();
    }
}
