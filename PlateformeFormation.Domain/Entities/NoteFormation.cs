
// ÉTAPE 2 — Entités Domain C#
// Fichier : PlateformeFormation.Domain/Entities/NoteFormation.cs

namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente la note (0.5 à 5.0) attribuée par un apprenant à une formation.
    // Un apprenant ne peut noter une formation qu'une seule fois.
    // Contrainte SQL : UQ_NoteFormation (UtilisateurId, FormationId).
    //
    public class NoteFormation
    {
        //Clé primaire auto-incrémentée.</summary>
        public int Id { get; set; }

        //ID de l'apprenant qui note. FK → Utilisateur.Id.</summary>
        public int UtilisateurId { get; set; }

        //ID de la formation notée. FK → Formation.Id.</summary>
        public int FormationId { get; set; }

        //
        // Note de 0.5 à 5.0 par pas de 0.5.
        // Exemples valides : 1.0, 1.5, 2.0, ..., 4.5, 5.0.
        // Contrainte SQL : CK_NoteFormation_Note.
        //
        public decimal Note { get; set; }

        //Date à laquelle la note a été soumise. Assignée par le serveur SQL.</summary>
        public DateTime DateNote { get; set; }
    }
}
