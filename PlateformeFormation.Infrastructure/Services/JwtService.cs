
// Infrastructure/Services/JwtService.cs
//
// Service responsable de la génération des tokens JWT.
// Source UNIQUE de génération — AuthController délègue ici.
//
// CORRECTION APPLIQUÉE :
//   ClaimTypes.Role contient maintenant l'ID numérique du rôle
//   (ex : "1", "2", "3") et non le nom textuel.
//   Cela est nécessaire pour que [Authorize(Roles = "1,2")]
//   fonctionne correctement dans les controllers.


using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Infrastructure.Services
{
    //
    // Service de génération de tokens JWT.
    // Enregistré comme Scoped dans Program.cs (lit la config à chaque requête).
    //
    public class JwtService
    {
        private readonly IConfiguration _config;

        //Injecte la configuration pour lire Jwt:Key, Jwt:Issuer, Jwt:Audience.</summary>
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        //
        // Génère un token JWT signé contenant les claims de l'utilisateur.
        //
        // Claims inclus :
        //   - "id"                      → ID utilisateur (claim custom, utilisé dans les controllers)
        //   - ClaimTypes.NameIdentifier → ID utilisateur (standard ASP.NET)
        //   - ClaimTypes.Name           → Nom de l'utilisateur
        //   - ClaimTypes.Role           → ID du rôle en string ("1", "2", "3")
        //                                  Nécessaire pour [Authorize(Roles = "1,2")]
        //   - "roleNom"                 → Nom lisible du rôle (info complémentaire pour le frontend)
        //
        // Durée de validité : 4 heures (configurée dans la construction du token).
        //
        // <param name="user">Utilisateur authentifié.</param>
        // <param name="roleNom">Nom du rôle (ex : "Admin", "Formateur", "Apprenant").</param>
        // <returns>Token JWT signé sous forme de chaîne Base64.</returns>
        public string GenererToken(Utilisateur user, string roleNom)
        {
            // 1) Lecture des paramètres depuis appsettings.json
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;

            // 2) Création de la clef de signature HMAC-SHA256
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 3) Définition des claims embarqués dans le token
            var claims = new[]
            {
                // Identifiant utilisateur — utilisé dans les controllers via :
                // int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                new Claim("id",                          user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),

                // Nom de l'utilisateur
                new Claim(ClaimTypes.Name,               user.Nom),

                // CORRECTION CRITIQUE : ID numérique du rôle (pas le nom textuel)
                // [Authorize(Roles = "1,2")] compare ce claim avec "1" et "2"
                // Si on mettait "Admin" ici, [Authorize(Roles = "1")] ne fonctionnerait pas.
                new Claim(ClaimTypes.Role,               user.RoleId.ToString()),

                // Nom lisible du rôle — utilisé par le frontend pour l'affichage
                new Claim("roleNom",                     roleNom)
            };

            // 4) Construction du token JWT
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: credentials
            );

            // 5) Sérialisation en chaîne Base64
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}