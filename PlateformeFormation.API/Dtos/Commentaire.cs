using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{


    
    // COMMENTAIRES
    

    //
    // DTO reçu par POST /api/Commentaire.
    // L'AuteurId est extrait du JWT — jamais fourni par le client.
    // FormationId OU FormateurId doit être renseigné (au moins un).
    //
    public class CommentaireCreateDto
    {
        //ID de la formation commentée. Nullable si le commentaire cible un formateur.</summary>
        public int? FormationId { get; set; }

        //ID du formateur commenté. Nullable si le commentaire cible une formation.</summary>
        public int? FormateurId { get; set; }

        //Contenu du commentaire (entre 10 et 2000 caractères).</summary>
        [Required(ErrorMessage = "Le contenu du commentaire est obligatoire.")]
        [MinLength(10, ErrorMessage = "Le commentaire doit contenir au moins 10 caractères.")]
        [MaxLength(2000, ErrorMessage = "Le commentaire ne peut pas dépasser 2000 caractères.")]
        public string Contenu { get; set; } = string.Empty;
    }

    //
    // DTO renvoyé au frontend avec les informations du commentaire.
    // Inclut le nom de l'auteur pour l'affichage.
    //
    public class CommentaireReadDto
    {
        public int Id { get; set; }
        public int AuteurId { get; set; }

        //Prénom + Nom de l'auteur — joint depuis la table Utilisateur.</summary>
        public string NomAuteur { get; set; } = string.Empty;

        public int? FormationId { get; set; }
        public int? FormateurId { get; set; }
        public string Contenu { get; set; } = string.Empty;
        public DateTime DateCommentaire { get; set; }
        public bool EstVisible { get; set; }
    }
}