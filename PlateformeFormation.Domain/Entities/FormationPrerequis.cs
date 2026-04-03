
// Domain/Entities/FormationPrerequis.cs
//
// Entité représentant un lien de prérequis entre deux formations.


namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente un lien de prérequis entre deux formations.
    // Signification : pour s'inscrire à FormationId, l'utilisateur
    // doit avoir TERMINÉ (statut = "Terminé") la FormationRequiseId.
    //
    // Relation N↔N entre formations.
    // Clé primaire composite (FormationId, FormationRequiseId) en SQL.
    // Contrainte CHECK empêche qu'une formation soit son propre prérequis.
    //
    public class FormationPrerequis
    {
        //
        // Identifiant de la formation cible (celle qui exige le prérequis).
        // Clé étrangère vers Formation. ON DELETE CASCADE.
        //
        public int FormationId { get; set; }

        //
        // Identifiant de la formation qui doit être terminée en prérequis.
        // Clé étrangère vers Formation.
        //
        public int FormationRequiseId { get; set; }
    }
}