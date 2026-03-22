namespace PlateformeFormation.API.Dtos
{
    
    // DTO renvoyé au client pour représenter un rôle.
    
    public class RoleReadDto
    {
        
        // Identifiant unique du rôle.
        
        public int Id { get; set; }

        
        // Nom du rôle.
        
        public string Nom { get; set; } = string.Empty;
    }
}
