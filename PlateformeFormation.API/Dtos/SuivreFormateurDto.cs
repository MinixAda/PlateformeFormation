using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    
    // SUIVI FORMATEUR
    

    //
    // DTO reçu par POST /api/SuiviFormateur pour s'abonner à un formateur.
    // L'ApprenantId est extrait du JWT — jamais fourni par le client.
    //
    public class SuivreFormateurDto
    {
        //ID du formateur à suivre.</summary>
        [Required(ErrorMessage = "L'ID du formateur est obligatoire.")]
        public int FormateurId { get; set; }
    }

    //
    // DTO renvoyé par GET /api/SuiviFormateur/mes-formateurs.
    // Informations sur chaque formateur suivi.
    //
    public class FormateurSuiviReadDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? LienPortfolio { get; set; }
        public DateTime DateSuivi { get; set; }
    }
}
