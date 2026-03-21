using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlateformeFormation.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlateformeFormation.Infrastructure.Services
{
    
    // Service responsable de la génération des tokens JWT.
    // Centralise toute la logique liée à l'authentification par token.
    
    public class JwtService
    {
        private readonly IConfiguration _config;

        
        // Injecte la configuration (appsettings.json) pour récupérer la clé JWT,
        // l'issuer et l'audience.
        
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        
        // Génère un token JWT signé contenant les informations essentielles
        // de l'utilisateur (id, nom, rôle).
        
        // <param name="user">Utilisateur authentifié</param>
        // <param name="roleNom">Nom du rôle (Administrateur, Formateur, Apprenant)</param>
        // <returns>Token JWT signé</returns>
        public string GenererToken(Utilisateur user, string roleNom)
        {
            // 1) Récupération des paramètres JWT depuis appsettings.json
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;

            // 2) Création de la clé de signature
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 3) Définition des claims (informations embarquées dans le token)
            var claims = new[]
            {
                // Identifiant utilisateur (utilisé par tes controllers)
                new Claim("id", user.Id.ToString()),

                // Identifiant standard (compatibilité ASP.NET)
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // Nom de l'utilisateur
                new Claim(ClaimTypes.Name, user.Nom),

                // Rôle utilisateur (Administrateur, Formateur, Apprenant)
                new Claim(ClaimTypes.Role, roleNom)
            };

            // 4) Construction du token JWT
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4), // Durée de validité
                signingCredentials: credentials
            );

            // 5) Conversion en string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
