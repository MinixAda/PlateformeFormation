// Fichier : PlateformeFormation.API/Dtos/NoteFormationDtos.cs


using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    
    // NOTATION
    

    //
    // DTO reçu par POST /api/NoteFormation.
    // L'utilisateur envoie uniquement la formation et sa note.
    // L'UtilisateurId est extrait du token JWT côté serveur.
    //
    public class NoteFormationCreateDto
    {
        //ID de la formation à noter.</summary>
        [Required(ErrorMessage = "L'ID de la formation est obligatoire.")]
        public int FormationId { get; set; }

        //
        // Note de 0.5 à 5.0 par pas de 0.5.
        // Exemples valides : 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0.
        //
        [Required(ErrorMessage = "La note est obligatoire.")]
        [Range(0.5, 5.0, ErrorMessage = "La note doit être entre 0.5 et 5.0.")]
        public decimal Note { get; set; }
    }
}