// Fichier : PlateformeFormation.Domain/Entities/SuiviFormateur.cs

namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente l'abonnement d'un apprenant à un formateur.
    // Un apprenant qui suit un formateur reçoit des notifications
    // quand ce formateur publie une nouvelle formation.
    // PK composite (ApprenantId, FormateurId) → pas de doublon.
    // Contrainte SQL : CK_SuiviFormateur_NoSelf (pas d'auto-suivi).
    //
    public class SuiviFormateur
    {
        //ID de l'apprenant abonné. FK → Utilisateur.Id.</summary>
        public int ApprenantId { get; set; }

        //ID du formateur suivi. FK → Utilisateur.Id.</summary>
        public int FormateurId { get; set; }

        //Date à laquelle l'abonnement a été créé.</summary>
        public DateTime DateSuivi { get; set; }
    }
}
