using System.ComponentModel.DataAnnotations;

namespace PlateformeFormation.API.Dtos
{
    public class ChangePasswordDto
    {
        public string AncienMotDePasse { get; set; } = string.Empty;
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }
}
