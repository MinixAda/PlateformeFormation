// Fichier : PlateformeFormation.Domain/Entities/Signalement.cs

namespace PlateformeFormation.Domain.Entities
{
    //
    // Signalement d'un contenu inapproprié (commentaire ou formation).
    // TypeCible : "Commentaire" ou "Formation".
    // CibleId   : l'ID du commentaire ou de la formation signalée.
    // Statut    : "EnAttente" (défaut) → "Traité" (admin a agi) ou "Rejeté" (non fondé).
    //
    public class Signalement
    {
        //Clé primaire auto-incrémentée.</summary>
        public int Id { get; set; }

        //ID de l'utilisateur qui signale. FK → Utilisateur.Id.</summary>
        public int SignaleurId { get; set; }

        //
        // Type de la cible signalée.
        // Valeurs valides : "Commentaire" | "Formation"
        // Contrainte SQL : CK_Signalement_TypeCible.
        //
        public string TypeCible { get; set; } = string.Empty;

        //
        // ID de l'élément signalé (ID du Commentaire ou de la Formation selon TypeCible).
        // Pas de FK SQL car la cible peut être de deux types différents.
        //
        public int CibleId { get; set; }

        //Description du motif du signalement (max 500 caractères).</summary>
        public string Motif { get; set; } = string.Empty;

        //Date du signalement. Assignée par le serveur SQL.</summary>
        public DateTime DateSignalement { get; set; }

        //
        // Statut de traitement du signalement.
        // "EnAttente" (défaut) → en attente de modération admin.
        // "Traité" → l'admin a pris une action (ex: masqué le commentaire).
        // "Rejeté" → l'admin a jugé le signalement non fondé.
        // Contrainte SQL : CK_Signalement_Statut.
        //
        public string Statut { get; set; } = "EnAttente";
    }
}