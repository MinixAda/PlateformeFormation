namespace PlateformeFormation.API.Dtos.Auth
{
    public class ChangePasswordDto
    {
        public string AncienMotDePasse { get; set; } = string.Empty;
        public string NouveauMotDePasse { get; set; } = string.Empty;
    }
}
