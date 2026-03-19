namespace PlateformeFormation.API.Dtos
{
    // DTO utilisé pour créer / mettre à jour un utilisateur
    public class UtilisateurCreateDto
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

       
        public string Password { get; set; } = string.Empty;

       
        public int RoleId { get; set; }
    }
}
