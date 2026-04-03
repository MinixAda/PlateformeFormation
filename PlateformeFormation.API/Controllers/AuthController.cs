
// API/Controllers/AuthController.cs
// Responsabilités :
//   POST /api/auth/login  --> connexion + token JWT
//   POST /api/auth/register --> inscription apprenant
//   POST /api/auth/changer-mot-de-passe --> changement MDP
//   GET  /api/auth/me --> profil connecté
//   PATCH /api/auth/profil --> mise à jour Bio + LienPortfolio
//
// CORRECTIONS APPLIQUÉES :
//   1. JwtService utilisé comme source unique (plus de GenerateJwtToken local)
//   2. Register() : RoleId supprimé du DTO, forcé à 3 (Apprenant) côté serveur
//   3. GET /me ajouté (requis par ProfilPage.tsx)
//   4. PATCH /profil ajouté (Bio + LienPortfolio — consignes TFE)
//   5. Gestion d'exceptions explicite sur chaque action


using System;
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
    public class AuthController : ControllerBase
    {
        private readonly IUtilisateurRepository _utilisateurRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;

        public AuthController(
            IUtilisateurRepository utilisateurRepo,
            IRoleRepository roleRepo,
            PasswordService passwordService,
            JwtService jwtService)
        {
            _utilisateurRepo = utilisateurRepo;
            _roleRepo = roleRepo;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        
        // POST /api/auth/login
        
        
        // Authentifie un utilisateur et retourne un token JWT.
        // La réponse inclut toutes les infos utiles au frontend
        // (Id, Nom, Prénom, RoleId, RoleNom) pour éviter des appels
        // API supplémentaires après connexion.
        
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // 1) Vérifier que l'email existe
                var user = await _utilisateurRepo.GetByEmailAsync(dto.Email);
                if (user == null)
                    return Unauthorized("Identifiants invalides.");

                // 2) Vérifier le mot de passe via BCrypt
                if (!_passwordService.VerifyPassword(dto.MotDePasse, user.MotDePasseHash))
                    return Unauthorized("Identifiants invalides.");

                // 3) Récupérer le rôle
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                if (role == null)
                    return StatusCode(500, "Rôle de l'utilisateur introuvable. Contactez l'administrateur.");

                // 4) Générer le token JWT via JwtService (source unique)
                var token = _jwtService.GenererToken(user, role.Nom);

                // 5) Retourner les infos complètes utiles au frontend
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

        
        // POST /api/auth/register
        
        
        // Inscription publique d'un nouvel apprenant.
        // Le rôle est TOUJOURS forcé à 3 (Apprenant) côté serveur —
        // on ne fait jamais confiance au client pour s'attribuer un rôle.
        
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                // Validation minimale des champs obligatoires
                if (string.IsNullOrWhiteSpace(dto.Nom))
                    return BadRequest("Le nom est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Prenom))
                    return BadRequest("Le prénom est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest("L'adresse email est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.MotDePasse))
                    return BadRequest("Le mot de passe est obligatoire.");
                if (dto.MotDePasse.Length < 8)
                    return BadRequest("Le mot de passe doit contenir au moins 8 caractères.");

                // Vérifier l'unicité de l'email
                var existing = await _utilisateurRepo.GetByEmailAsync(dto.Email.Trim());
                if (existing != null)
                    return BadRequest("Un compte avec cette adresse email existe déjà.");

                // Récupérer le rôle Apprenant (Id = 3)
                var role = await _roleRepo.GetByIdAsync(3);
                if (role == null)
                    return StatusCode(500,
                        "Le rôle Apprenant (Id=3) est introuvable en base. " +
                        "Exécutez le script SQL d'initialisation.");

                // Créer l'utilisateur
                var user = new Utilisateur
                {
                    Nom = dto.Nom.Trim(),
                    Prenom = dto.Prenom.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    MotDePasseHash = _passwordService.HashPassword(dto.MotDePasse),
                    RoleId = 3  // Apprenant — forcé côté serveur
                };

                await _utilisateurRepo.CreateAsync(user);

                return Ok("Compte créé avec succès. Vous pouvez maintenant vous connecter.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création du compte : {ex.Message}");
            }
        }

        
        // POST /api/auth/changer-mot-de-passe
        
        
        // Permet à l'utilisateur connecté de changer son mot de passe.
        // Nécessite l'ancien mot de passe pour confirmation de l'identité.
        
        [Authorize]
        [HttpPost("changer-mot-de-passe")]
        public async Task<ActionResult> ChangerMotDePasse([FromBody] ChangePasswordDto dto)
        {
            try
            {
                // Validation des champs
                if (string.IsNullOrWhiteSpace(dto.AncienMotDePasse))
                    return BadRequest("L'ancien mot de passe est obligatoire.");
                if (string.IsNullOrWhiteSpace(dto.NouveauMotDePasse))
                    return BadRequest("Le nouveau mot de passe est obligatoire.");
                if (dto.NouveauMotDePasse.Length < 8)
                    return BadRequest("Le nouveau mot de passe doit contenir au moins 8 caractères.");

                // Extraire l'ID depuis le token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

                // Charger l'utilisateur
                var user = await _utilisateurRepo.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                // Vérifier l'ancien mot de passe
                if (!_passwordService.VerifyPassword(dto.AncienMotDePasse, user.MotDePasseHash))
                    return BadRequest("L'ancien mot de passe est incorrect.");

                // Hasher et sauvegarder le nouveau mot de passe
                user.MotDePasseHash = _passwordService.HashPassword(dto.NouveauMotDePasse);
                await _utilisateurRepo.UpdatePasswordAsync(user);

                return Ok("Mot de passe mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du changement de mot de passe : {ex.Message}");
            }
        }

        
        // GET /api/auth/me               
        // Retourne les informations complètes de l'utilisateur connecté.
        // L'ID est extrait du token JWT — jamais passé en paramètre URL
        // (ce serait une faille : un utilisateur pourrait demander le profil d'un autre).
        
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> Me()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

                var user = await _utilisateurRepo.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

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
                return StatusCode(500, $"Erreur lors de la récupération du profil : {ex.Message}");
            }
        }

        
        // PATCH /api/auth/profil
               
        // Met à jour les informations de profil de l'utilisateur connecté :
        // Bio et LienPortfolio.
        // "Page de profil avec possibilité de lien vers portfolio (git)".
        // Patch (et non PUT) car seuls deux champs sont modifiables ici.
        
        [Authorize]
        [HttpPatch("profil")]
        public async Task<ActionResult> UpdateProfil([FromBody] UpdateProfilDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

                var user = await _utilisateurRepo.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Utilisateur introuvable.");

                // Mise à jour des champs de profil
                user.Bio = dto.Bio;
                user.LienPortfolio = dto.LienPortfolio;

                await _utilisateurRepo.UpdateProfilAsync(user);

                return Ok("Profil mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour du profil : {ex.Message}");
            }
        }
    }
}