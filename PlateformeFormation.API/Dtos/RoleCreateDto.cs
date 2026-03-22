namespace PlateformeFormation.API.Dtos
{
    
    // DTO utilisé pour créer un nouveau rôle.
    
    public class RoleCreateDto
    {
        
        // Nom du rôle (ex : Admin, Formateur, Apprenant).
        
        public string Nom { get; set; } = string.Empty;
    }
}
