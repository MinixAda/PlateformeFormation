using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Services;

namespace PlateformeFormation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateursController : ControllerBase
    {
        private readonly IUtilisateurRepository _repo;
        private readonly PasswordService _passwordService;

        public UtilisateursController(
            IUtilisateurRepository repo,
            PasswordService passwordService)
        {
            _repo = repo;
            _passwordService = passwordService;
        }

        
        // GET : /api/Utilisateurs
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _repo.GetAllAsync();
            return Ok(users);
        }

        
        // GET : /api/Utilisateurs/{id}
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return NotFound("Utilisateur introuvable");

            return Ok(user);
        }

        
        // POST : /api/Utilisateurs
        // Création avec HASH automatique
        
        [HttpPost]
        public async Task<IActionResult> Create(UtilisateurCreateDto dto)
        {
            // Vérifier si l'email existe déjà
            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null)
                return BadRequest("Un utilisateur avec cet email existe déjà.");

            // Hash du mot de passe
            var hashedPassword = _passwordService.HashPassword(dto.Password);

            // Création de l'entité
            var user = new Utilisateur
            {
                Nom = dto.Nom,
                Prenom = dto.Prenom,
                Email = dto.Email,
                MotDePasseHash = hashedPassword,
                RoleId = dto.RoleId // Int, pas enum
            };

            // Appel au repository
            await _repo.CreateAsync(user);

            return Ok(new
            {
                message = "Utilisateur créé avec succès",
                utilisateur = user
            });
        }

        
        // Put : /api/Utilisateurs/{id}
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UtilisateurCreateDto dto)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return NotFound("Utilisateur introuvable");

            // Hash du mot de passe (si modifié)
            var hashedPassword = _passwordService.HashPassword(dto.Password);

            user.Nom = dto.Nom;
            user.Prenom = dto.Prenom;
            user.Email = dto.Email;
            user.MotDePasseHash = hashedPassword;
            user.RoleId = dto.RoleId;

            await _repo.UpdateAsync(user);

            return Ok(new { message = "Utilisateur mis à jour", utilisateur = user });
        }

        
        // Delete : /api/Utilisateurs/{id}
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return NotFound("Utilisateur introuvable");

            await _repo.DeleteAsync(id);

            return Ok(new { message = "Utilisateur supprimé" });
        }
    }
}
