

// API/Dtos/RoleCreateDto.cs

namespace PlateformeFormation.API.Dtos
{
    // DTO utilisé pour créer un nouveau rôle.

    //DTO reçu par POST /api/Role.</summary>
    public class RoleCreateDto
    {
        //Nom du rôle (ex : "Admin", "Formateur", "Apprenant").</summary>
        public string Nom { get; set; } = string.Empty;
    }
}