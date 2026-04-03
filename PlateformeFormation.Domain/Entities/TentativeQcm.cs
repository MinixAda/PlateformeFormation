
// Domain/Entities/TentativeQcm.cs


using System;

namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente une tentative de QCM effectuée par un apprenant sur un module.
    // Permet de tracer l'historique des tentatives et les scores obtenus.
    // Un apprenant peut faire plusieurs tentatives sur le même QCM.
    //
    public class TentativeQcm
    {
        //Identifiant unique de la tentative.</summary>
        public int Id { get; set; }

        //Identifiant de l'utilisateur ayant passé le QCM.</summary>
        public int UtilisateurId { get; set; }

        //Identifiant du module dont le QCM a été passé.</summary>
        public int ModuleId { get; set; }

        //Nombre de bonnes réponses obtenues.</summary>
        public int Score { get; set; }

        //Nombre total de questions du QCM au moment de la tentative.</summary>
        public int Total { get; set; }

        //
        // Indique si la tentative est réussie (score >= seuil de réussite).
        // Seuil par défaut : 60% (configurable dans QcmController).
        //
        public bool Reussi { get; set; }

        //Date et heure de la tentative (assignée côté serveur).</summary>
        public DateTime DateTentative { get; set; }
    }
}