
// Domain/Entities/Reponse.cs


namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente une réponse possible à une question de QCM.
    // Chaque question a plusieurs réponses dont une seule est correcte.
    //
    // SÉCURITÉ : EstCorrecte n'est JAMAIS envoyé au frontend
    // (il est exclu des DTOs de lecture — ReponseReadDto).
    // Seul QcmController l'utilise côté serveur pour la correction.
    //
    public class Reponse
    {
        //Identifiant unique de la réponse.</summary>
        public int Id { get; set; }

        //
        // Identifiant de la question parente.
        // Clé étrangère vers Question. ON DELETE CASCADE.
        //
        public int QuestionId { get; set; }

        //Texte de la réponse affiché à l'apprenant.</summary>
        public string Texte { get; set; } = string.Empty;

        //
        // Indique si cette réponse est la bonne réponse.
        // NE PAS exposer dans les DTOs de lecture (ReponseReadDto).
        // Utilisé uniquement par QcmController.ValiderQcm() côté serveur.
        //
        public bool EstCorrecte { get; set; }
    }
}