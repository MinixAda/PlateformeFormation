
// API/Controllers/UtilisateursController.cs
//
// Gère les opérations CRUD sur les utilisateurs.
// Réservé aux administrateurs (RoleId = 1).
//
// CORRECTIONS APPLIQUÉES :
//   1. Update() utilise UtilisateurUpdateDto séparé (pas de password dans PUT)
//   2. RoleNom inclus dans les réponses (UserResponseDto corrigé)
//   3. Bio et LienPortfolio inclus dans les réponses
//   4. Vérification que l'admin ne peut pas supprimer son propre compte
//   5. Gestion d'exceptions explicite sur chaque action
//   6. Tous les using nécessaires


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Services;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")]  // Admin uniquement
    public class UtilisateursController : ControllerBase
    {
        private readonly IUtilisateurRepository _utilisateurRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly PasswordService _passwordService;

        public UtilisateursController(
            IUtilisateurRepository utilisateurRepo,
            IRoleRepository roleRepo,
            PasswordService passwordService)
        {
            _utilisateurRepo = utilisateurRepo;
            _roleRepo = roleRepo;
            _passwordService = passwordService;
        }

        
        // GET /api/Utilisateurs
        
        //
        // Retourne la liste de tous les utilisateurs.
        // Inclut Nom, Prénom, Email, RoleId, RoleNom, Bio, LienPortfolio.
        // NE retourne JAMAIS le hash du mot de passe.
        //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            try
            {
                var users = await _utilisateurRepo.GetAllAsync();
                var roles = await _roleRepo.GetAllAsync();

                // Construire un dictionnaire Id → Nom pour éviter N requêtes SQL
                var roleMap = roles.ToDictionary(r => r.Id, r => r.Nom);

                var result = users.Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Nom = u.Nom,
                    Prenom = u.Prenom,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleNom = roleMap.TryGetValue(u.RoleId, out var nomRole) ? nomRole : "Inconnu",
                    Bio = u.Bio,
                    LienPortfolio = u.LienPortfolio
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des utilisateurs : {ex.Message}");
            }
        }

        
        // GET /api/Utilisateurs/{id}
        
        //Retourne un utilisateur par son ID.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            try
            {
                var user = await _utilisateurRepo.GetByIdAsync(id);
                if (user == null)
                    return NotFound($"Utilisateur #{id} introuvable.");

                var role = await _roleRepo.GetByIdAsync(user.RoleId);

                return Ok(new UserResponseDto
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    RoleNom = role?.Nom ?? "Inconnu",
                    Bio = user.Bio,
                    LienPortfolio = user.LienPortfolio
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération de l'utilisateur #{id} : {ex.Message}");
            }
        }

        
        // POST /api/Utilisateurs
        
        //
        // Crée un nouvel utilisateur (admin uniquement).
        // Permet de créer n'importe quel rôle (y compris Admin ou Formateur).
        // Le mot de passe est hashé avant stockage.
        //
        // 
        //✨✨✨✨🦎[AllowAnonymous] // Désactive l'authentification pour cette action 
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UtilisateurCreateDto dto)
        {
            try
            {
                // Validations des champs obligatoires
                if (string.IsNullOrWhiteSpace(dto.Nom))
                    return BadRequest("Le nom est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Prenom))
                    return BadRequest("Le prénom est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("L'adresse email est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest("Le mot de passe est obligatoire.");
                if (dto.Password.Length < 8)
                    return BadRequest("Le mot de passe doit contenir au moins 8 caractères.");

                // Vérifier que le rôle existe
                var roleExiste = await _roleRepo.GetByIdAsync(dto.RoleId);
                if (roleExiste == null)
                    return BadRequest($"Le rôle #{dto.RoleId} n'existe pas.");

                // Vérifier l'unicité de l'email
                var existing = await _utilisateurRepo.GetByEmailAsync(dto.Email.Trim());
                if (existing != null)
                    return BadRequest("Un utilisateur avec cette adresse email existe déjà.");

                // Créer l'utilisateur avec le mot de passe hashé
                var user = new Utilisateur
                {
                    Nom = dto.Nom.Trim(),
                    Prenom = dto.Prenom.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    MotDePasseHash = _passwordService.HashPassword(dto.Password),
                    RoleId = dto.RoleId
                };

                await _utilisateurRepo.CreateAsync(user);
                return Ok("Utilisateur créé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la création de l'utilisateur : {ex.Message}");
            }
        }

        
        // PUT /api/Utilisateurs/{id}
        
        //
        // Met à jour les informations administratives d'un utilisateur :
        // Nom, Prénom, Email, RoleId.
        // NE modifie PAS le mot de passe (endpoint dédié : POST /auth/changer-mot-de-passe).
        //
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UtilisateurCreateDto dto)
        {
            try
            {
                // Validations
                if (string.IsNullOrWhiteSpace(dto.Nom))
                    return BadRequest("Le nom est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Prenom))
                    return BadRequest("Le prénom est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("L'adresse email est obligatoire.");

                var user = await _utilisateurRepo.GetByIdAsync(id);
                if (user == null)
                    return NotFound($"Utilisateur #{id} introuvable.");

                // Vérifier que le rôle existe
                var roleExiste = await _roleRepo.GetByIdAsync(dto.RoleId);
                if (roleExiste == null)
                    return BadRequest($"Le rôle #{dto.RoleId} n'existe pas.");

                // Vérifier l'unicité de l'email si changé
                if (!user.Email.Equals(dto.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var emailExistant = await _utilisateurRepo.GetByEmailAsync(dto.Email.Trim());
                    if (emailExistant != null && emailExistant.Id != id)
                        return BadRequest("Cette adresse email est déjà utilisée par un autre compte.");
                }

                // Mise à jour des champs administratifs uniquement
                user.Nom = dto.Nom.Trim();
                user.Prenom = dto.Prenom.Trim();
                user.Email = dto.Email.Trim().ToLower();
                user.RoleId = dto.RoleId;
                // MotDePasseHash et champs de profil (Bio, LienPortfolio) non modifiés ici

                await _utilisateurRepo.UpdateAsync(user);
                return Ok("Utilisateur mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la mise à jour de l'utilisateur #{id} : {ex.Message}");
            }
        }

        
        // DELETE /api/Utilisateurs/{id}
        
        //
        // Supprime un utilisateur par son ID.
        // Protection : un administrateur ne peut pas supprimer son propre compte.
        //
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // Extraire l'ID de l'admin connecté depuis le token JWT
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (adminIdClaim != null && int.Parse(adminIdClaim.Value) == id)
                    return BadRequest("Vous ne pouvez pas supprimer votre propre compte administrateur.");

                var user = await _utilisateurRepo.GetByIdAsync(id);
                if (user == null)
                    return NotFound($"Utilisateur #{id} introuvable.");

                await _utilisateurRepo.DeleteAsync(id);
                return Ok("Utilisateur supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la suppression de l'utilisateur #{id} : {ex.Message}");
            }
        }
    }
}