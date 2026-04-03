using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{

    
    // SIGNALEMENTS
    

    //
    // DTO reçu par POST /api/Signalement.
    // Le SignaleurId est extrait du JWT.
    //
    public class SignalementCreateDto
    {
        //
        // Type de la cible signalée.
        // Valeurs acceptées : "Commentaire" | "Formation"
        //
        [Required(ErrorMessage = "Le type de cible est obligatoire.")]
        [RegularExpression("^(Commentaire|Formation)$",
            ErrorMessage = "TypeCible doit être 'Commentaire' ou 'Formation'.")]
        public string TypeCible { get; set; } = string.Empty;

        //ID du commentaire ou de la formation signalé.</summary>
        [Required(ErrorMessage = "L'ID de la cible est obligatoire.")]
        public int CibleId { get; set; }

        //Description du motif du signalement (10 à 500 caractères).</summary>
        [Required(ErrorMessage = "Le motif du signalement est obligatoire.")]
        [MinLength(10, ErrorMessage = "Le motif doit contenir au moins 10 caractères.")]
        [MaxLength(500, ErrorMessage = "Le motif ne peut pas dépasser 500 caractères.")]
        public string Motif { get; set; } = string.Empty;
    }

    //
    // DTO renvoyé à l'admin pour la liste des signalements.
    //
    public class SignalementReadDto
    {
        public int Id { get; set; }
        public int SignaleurId { get; set; }
        public string NomSignaleur { get; set; } = string.Empty;
        public string TypeCible { get; set; } = string.Empty;
        public int CibleId { get; set; }
        public string Motif { get; set; } = string.Empty;
        public DateTime DateSignalement { get; set; }
        public string Statut { get; set; } = string.Empty;
    }

    //
    // DTO reçu par PATCH /api/Signalement/{id}/statut (admin uniquement).
    //
    public class SignalementUpdateStatutDto
    {
        //Valeurs valides : "Traité" | "Rejeté"</summary>
        [Required(ErrorMessage = "Le statut est obligatoire.")]
        [RegularExpression("^(Traité|Rejeté)$",
            ErrorMessage = "Statut doit être 'Traité' ou 'Rejeté'.")]
        public string Statut { get; set; } = string.Empty;
    }










}
