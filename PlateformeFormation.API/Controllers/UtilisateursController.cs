using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Services;

namespace PlateformeFormation.API.Controllers
{
    
    // Gère les opérations CRUD sur les utilisateurs.
    // Réservé aux administrateurs (RoleId = 1).
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")] // Admin uniquement
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

        
        // Récupère la liste de tous les utilisateurs.
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            try
            {
                var users = await _repo.GetAllAsync();

                // Mapping entité -> DTO de réponse
                var result = users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Nom = u.Nom,
                    Prenom = u.Prenom,
                    Email = u.Email,
                    RoleId = u.RoleId
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Erreur serveur générique, message explicite
                return StatusCode(500, $"Erreur lors de la récupération des utilisateurs : {ex.Message}");
            }
        }

        
        // Récupère un utilisateur par son identifiant.
        
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            try
            {
                var user = await _repo.GetByIdAsync(id);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                var dto = new UserResponseDto
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email,
                    RoleId = user.RoleId
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de l'utilisateur : {ex.Message}");
            }
        }

        
        // Crée un nouvel utilisateur.
        // Le mot de passe est hashé avant stockage.
        
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UtilisateurCreateDto dto)
        {
            try
            {
                // Vérifier l'unicité de l'email
                var existing = await _repo.GetByEmailAsync(dto.Email);
                if (existing != null)
                    return BadRequest("Un utilisateur avec cet email existe déjà.");

                // Hash du mot de passe
                var hash = _passwordService.HashPassword(dto.Password);

                var user = new Utilisateur
                {
                    Nom = dto.Nom,
                    Prenom = dto.Prenom,
                    Email = dto.Email,
                    MotDePasseHash = hash,
                    RoleId = dto.RoleId
                };

                await _repo.CreateAsync(user);
                return Ok("Utilisateur créé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de l'utilisateur : {ex.Message}");
            }
        }

        
        // Met à jour les informations d'un utilisateur.
        // Ne change pas le mot de passe ici.
        
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UtilisateurCreateDto dto)
        {
            try
            {
                var user = await _repo.GetByIdAsync(id);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                // Mise à jour des champs de base
                user.Nom = dto.Nom;
                user.Prenom = dto.Prenom;
                user.Email = dto.Email;
                user.RoleId = dto.RoleId;

                await _repo.UpdateAsync(user);
                return Ok("Utilisateur mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de l'utilisateur : {ex.Message}");
            }
        }

        
        // Supprime un utilisateur par son identifiant.
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var user = await _repo.GetByIdAsync(id);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                await _repo.DeleteAsync(id);
                return Ok("Utilisateur supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de l'utilisateur : {ex.Message}");
            }
        }
    }
}
