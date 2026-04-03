

// API/Dtos/RoleReadDto.cs

namespace PlateformeFormation.API.Dtos
{
    //DTO renvoyé par GET /api/Role et GET /api/Role/{id}.</summary>
    public class RoleReadDto
    {
        // Identifiant unique du rôle.

        public int Id { get; set; }

        // Nom du rôle.

        public string Nom { get; set; } = string.Empty;
    }
}