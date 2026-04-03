
// Domain/Entities/Question.cs
// Domain/Entities/Reponse.cs
// Domain/Entities/TentativeQcm.cs
//
// Entités pour le système de QCM — exigé par les consignes TFE :
// "Création des QCM et exercices" / "validation de compétences (QCM)"


// ---- Question.cs -------------------------------------------
namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente une question appartenant au QCM d'un module.
    // Chaque module peut avoir plusieurs questions.
    //
    public class Question
    {
        //Identifiant unique de la question.</summary>
        public int Id { get; set; }

        //
        // Identifiant du module auquel appartient cette question.
        // Clé étrangère vers Module. ON DELETE CASCADE.
        //
        public int ModuleId { get; set; }

        //Texte de la question affiché à l'apprenant.</summary>
        public string Texte { get; set; } = string.Empty;

        //Ordre d'affichage de la question dans le QCM.</summary>
        public int Ordre { get; set; }
    }
}