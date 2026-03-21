using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlateformeFormation.API.Dtos.Auth;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlateformeFormation.API.Controllers
{
    
    // Controller responsable de l'authentification, de la génération des tokens JWT
    // et de la gestion des mots de passe utilisateurs.
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUtilisateurRepository _utilisateurRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _config;

        public AuthController(
            IUtilisateurRepository utilisateurRepo,
            IRoleRepository roleRepo,
            PasswordService passwordService,
            IConfiguration config)
        {
            _utilisateurRepo = utilisateurRepo;
            _roleRepo = roleRepo;
            _passwordService = passwordService;
            _config = config;
        }

       
        // Login
        // Authentifie un utilisateur et renvoie un token JWT si les identifiants sont valides.
        
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // 1) Vérifier si l'utilisateur existe
                var user = await _utilisateurRepo.GetByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized("Identifiants invalides.");

                // 2) Vérifier le mot de passe via BCrypt
                if (!_passwordService.VerifyPassword(dto.MotDePasse, user.MotDePasseHash))
                    return Unauthorized("Identifiants invalides.");

                // 3) Récupérer le rôle associé
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                if (role == null)
                    return StatusCode(500, "Rôle utilisateur introuvable.");

                // 4) Générer le token JWT
                var token = GenerateJwtToken(user.Id, user.Nom, role.Id);

                // 5) Retourner les informations utiles au front
                return Ok(new
                {
                    Token = token,
                    UtilisateurId = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    RoleId = role.Id,
                    RoleNom = role.Nom
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'authentification : {ex.Message}");
            }
        }


        // Changer mot de passe
        // Permet à l'utilisateur connecté (via JWT) de changer son mot de passe.

        [Authorize]
        [HttpPost("changer-mot-de-passe")]
        public async Task<ActionResult> ChangerMotDePasse([FromBody] ChangePasswordDto dto)
        {
            try
            {
                // 1) Récupérer l'ID utilisateur depuis le token JWT
                var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
                if (userId == 0)
                    return Unauthorized("Utilisateur non authentifié.");

                // 2) Charger l'utilisateur
                var user = await _utilisateurRepo.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                // 3) Vérifier l'ancien mot de passe
                if (!_passwordService.VerifyPassword(dto.AncienMotDePasse, user.MotDePasseHash))
                    return BadRequest("L'ancien mot de passe est incorrect.");

                // 4) Générer le nouveau hash
                user.MotDePasseHash = _passwordService.HashPassword(dto.NouveauMotDePasse);

                // 5) Mise à jour en base
                await _utilisateurRepo.UpdatePasswordAsync(user);

                return Ok("Mot de passe mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du changement de mot de passe : {ex.Message}");
            }
        }


        // Tout ce qui suit est à usage de développement uniquement
        // Ne pas exposer en production
        // Token de test (dev only)
        // Génère un token JWT pour un utilisateur donné (utile en développement).

        [HttpGet("token-test/{userId}")]
        public async Task<ActionResult> GenererTokenTest(int userId)
        {
            try
            {
                var user = await _utilisateurRepo.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                if (role == null)
                    return StatusCode(500, "Rôle utilisateur introuvable.");

                var token = GenerateJwtToken(user.Id, user.Nom, role.Id);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la génération du token : {ex.Message}");
            }
        }

       
        // Génération du JWT
              
        // Génère un JWT signé contenant les informations essentielles de l'utilisateur.
        
        private string GenerateJwtToken(int userId, string nom, int roleId)
        {
            // 1) Lecture des paramètres JWT depuis appsettings.json
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;

            // 2) Création de la clef de signature
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 3) Claims embarqués dans le token
            var claims = new[]
            {
                new Claim("id", userId.ToString()),                    // ID utilisateur
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // ID standard
                new Claim(ClaimTypes.Name, nom),                        // Nom utilisateur
                new Claim(ClaimTypes.Role, roleId.ToString())           // Rôle
            };

            // 4) Construction du token
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
