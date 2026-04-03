
// ÉTAPE 2 (suite) — Entité Commentaire
// Fichier : PlateformeFormation.Domain/Entities/Commentaire.cs

namespace PlateformeFormation.Domain.Entities
{
    //
    // Commentaire posté par un utilisateur.
    // Peut cibler une Formation (FormationId renseigné) OU un Formateur
    // (FormateurId renseigné). Les deux peuvent être renseignés simultanément.
    // Au moins l'un des deux doit être non-null (contrainte SQL CK_Commentaire_Cible).
    // EstVisible permet à un admin de masquer un commentaire signalé sans le supprimer.
    //
    public class Commentaire
    {
        //Clé primaire auto-incrémentée.</summary>
        public int Id { get; set; }

        //ID de l'auteur du commentaire. FK → Utilisateur.Id.</summary>
        public int AuteurId { get; set; }

        //ID de la formation ciblée. Nullable si cible = formateur uniquement.</summary>
        public int? FormationId { get; set; }

        //ID du formateur ciblé. Nullable si cible = formation uniquement.</summary>
        public int? FormateurId { get; set; }

        //Contenu textuel du commentaire (max 2000 caractères).</summary>
        public string Contenu { get; set; } = string.Empty;

        //Date de publication. Assignée par le serveur SQL (DEFAULT GETDATE()).</summary>
        public DateTime DateCommentaire { get; set; }

        //
        // Visibilité du commentaire.
        // true = visible (défaut).
        // false = masqué par un admin suite à un signalement traité.
        //
        public bool EstVisible { get; set; } = true;
    }
}
