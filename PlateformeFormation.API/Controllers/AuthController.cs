using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PlateformeFormation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUtilisateurRepository _repo;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _config;

        public AuthController(
            IUtilisateurRepository repo,
            PasswordService passwordService,
            IConfiguration config)
        {
            _repo = repo;
            _passwordService = passwordService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);

            if (user == null)
                return Unauthorized("Email inconnu");

            if (!_passwordService.VerifyPassword(dto.Password, user.MotDePasseHash))
                return Unauthorized("Mot de passe incorrect");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new Exception("La clé JWT 'Jwt:Key' est manquante dans appsettings.json");
            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new Exception("Le paramètre 'Jwt:Issuer' est manquant dans appsettings.json");
            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new Exception("Le paramètre 'Jwt:Audience' est manquant dans appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest("Un utilisateur avec cet email existe déjà.");

            var hashedPassword = _passwordService.HashPassword(dto.Password);

            var user = new Utilisateur
            {
                Email = dto.Email,
                Nom = dto.Nom,
                Prenom = dto.Prenom,
                MotDePasseHash = hashedPassword,
                RoleId = dto.RoleId
            };

            // Appel à CreateAsync (aligné sur l'interface + repository)
            await _repo.CreateAsync(user);

            return Ok(new
            {
                message = "Utilisateur créé avec succès",
                utilisateur = new
                {
                    user.Id,
                    user.Email,
                    user.Nom,
                    user.Prenom,
                    user.RoleId
                }
            });
        }
    }
}
