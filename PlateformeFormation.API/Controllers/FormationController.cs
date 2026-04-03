
// API/Controllers/FormationController.cs
//
// CORRECTIONS APPLIQUÉES :
//   1. Correction des accès aux tuples nullable : userInfo.Value.UserId / RoleId
//   2. GetAll() : filtres par visibilité selon rôle (publiques visibles par tous)
//   3. GetAll() supporte ?q= ?niveau= ?langue=
//   4. GetById() : visibilité pour privées
//   5. Create/Update : EstPublique assigné depuis DTO
//   6. GetModules() retourne ModuleReadDto
//   7. Gestion d'exceptions explicite


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

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1,2")]  // Admin (1) + Formateur (2) pour actions d'écriture
    public class FormationController : ControllerBase
    {
        private readonly IFormationRepository _formationRepo;
        private readonly IModuleRepository _moduleRepo;

        public FormationController(
            IFormationRepository formationRepo,
            IModuleRepository moduleRepo)
        {
            _formationRepo = formationRepo ?? throw new ArgumentNullException(nameof(formationRepo));
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
        }

        
        // GET /api/Formation
        
        //
        // Retourne les formations selon visibilité/rôle :
        // - Anonyme/Apprenant : publiques uniquement
        // - Formateur : ses propres + toutes publiques
        // - Admin : toutes
        // Filtres : ?q= ?niveau= ?langue=
        //
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FormationReadDto>>> GetAll(
            [FromQuery] string? q = null,
            [FromQuery] string? niveau = null,
            [FromQuery] string? langue = null)
        {
            try
            {
                var formations = await _formationRepo.GetAllAsync();

                // Récupère infos utilisateur (nullable car AllowAnonymous)
                var userInfo = TryGetCurrentUser();

                // Filtre par visibilité selon rôle
                var filtered = formations.Where(f =>
                {
                    // Publique → visible par tous
                    if (f.EstPublique) return true;

                    // Privée → Admin : toujours visible
                    if (userInfo?.RoleId == 1) return true;

                    // Privée → Formateur : seulement si c'est la sienne
                    if (userInfo?.RoleId == 2 && f.CreateurId == userInfo.Value.UserId) return true;

                    return false;
                });

                // Filtre mot-clé (titre + description)
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var ql = q.ToLowerInvariant();
                    filtered = filtered.Where(f =>
                        f.Titre.ToLowerInvariant().Contains(ql) ||
                        (f.Description?.ToLowerInvariant().Contains(ql) ?? false));
                }

                // Filtre niveau
                if (!string.IsNullOrWhiteSpace(niveau))
                    filtered = filtered.Where(f => string.Equals(f.Niveau, niveau, StringComparison.OrdinalIgnoreCase));

                // Filtre langue
                if (!string.IsNullOrWhiteSpace(langue))
                    filtered = filtered.Where(f => string.Equals(f.Langue, langue, StringComparison.OrdinalIgnoreCase));

                return Ok(filtered.Select(MapToDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur récupération formations : {ex.Message}");
            }
        }

        
        // GET /api/Formation/{id}
        
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FormationReadDto>> GetById(int id)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                // Vérifier visibilité si privée
                if (!formation.EstPublique)
                {
                    var userInfo = TryGetCurrentUser();
                    bool peutVoir = false;

                    if (userInfo.HasValue)
                    {
                        peutVoir = userInfo.Value.RoleId == 1 ||  // Admin
                                  (userInfo.Value.RoleId == 2 && formation.CreateurId == userInfo.Value.UserId); // Formateur propriétaire
                    }

                    if (!peutVoir)
                        return NotFound("Formation introuvable.");
                }

                return Ok(MapToDto(formation));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur formation #{id} : {ex.Message}");
            }
        }

        
        // POST /api/Formation
        
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FormationCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Titre))
                    return BadRequest("Titre obligatoire.");

                var userInfo = GetCurrentUser(); // Lance exception si non authentifié

                var formation = new Formation
                {
                    Titre = dto.Titre.Trim(),
                    Description = dto.Description?.Trim(),
                    DateCreation = DateTime.UtcNow,
                    CreateurId = userInfo.UserId,
                    MediaType = dto.MediaType,
                    ModeDiffusion = dto.ModeDiffusion,
                    Langue = dto.Langue,
                    Niveau = dto.Niveau,
                    Prerequis = dto.Prerequis,
                    ImageUrl = dto.ImageUrl,
                    DureeMinutes = dto.DureeMinutes,
                    EstPublique = dto.EstPublique
                };

                await _formationRepo.CreateAsync(formation);
                return CreatedAtAction(nameof(GetById), new { id = formation.Id }, "Formation créée.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur création formation : {ex.Message}");
            }
        }

        
        // PUT /api/Formation/{id}
        
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] FormationUpdateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Titre))
                    return BadRequest("Titre obligatoire.");

                var formation = await _formationRepo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var userInfo = GetCurrentUser();

                // Formateur ne modifie que ses formations
                if (userInfo.RoleId == 2 && formation.CreateurId != userInfo.UserId)
                    return StatusCode(403, "Seules vos formations.");

                // Mise à jour
                formation.Titre = dto.Titre.Trim();
                formation.Description = dto.Description?.Trim();
                formation.MediaType = dto.MediaType;
                formation.ModeDiffusion = dto.ModeDiffusion;
                formation.Langue = dto.Langue;
                formation.Niveau = dto.Niveau;
                formation.Prerequis = dto.Prerequis;
                formation.ImageUrl = dto.ImageUrl;
                formation.DureeMinutes = dto.DureeMinutes;
                formation.EstPublique = dto.EstPublique;

                await _formationRepo.UpdateAsync(formation);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur mise à jour #{id} : {ex.Message}");
            }
        }

        
        // DELETE /api/Formation/{id}
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var userInfo = GetCurrentUser();

                if (userInfo.RoleId == 2 && formation.CreateurId != userInfo.UserId)
                    return StatusCode(403, "Seules vos formations.");

                await _formationRepo.DeleteAsync(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur suppression #{id} : {ex.Message}");
            }
        }

        
        // GET /api/Formation/{formationId}/modules
        
        [HttpGet("{formationId}/modules")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ModuleReadDto>>> GetModules(int formationId)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var modules = await _moduleRepo.GetByFormationIdAsync(formationId);
                var result = modules.OrderBy(m => m.Ordre).Select(m => new ModuleReadDto
                {
                    Id = m.Id,
                    FormationId = m.FormationId,
                    Titre = m.Titre,
                    Description = m.Description,
                    Ordre = m.Ordre,
                    DureeMinutes = m.DureeMinutes
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur modules #{formationId} : {ex.Message}");
            }
        }

        
        // POST /api/Formation/{formationId}/modules
        
        [HttpPost("{formationId}/modules")]
        public async Task<ActionResult> CreateModule(int formationId, [FromBody] ModuleCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Titre))
                    return BadRequest("Titre obligatoire.");

                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var userInfo = GetCurrentUser();
                if (userInfo.RoleId == 2 && formation.CreateurId != userInfo.UserId)
                    return StatusCode(403, "Vos formations seulement.");

                var module = new Module
                {
                    FormationId = formationId,
                    Titre = dto.Titre.Trim(),
                    Description = dto.Description?.Trim(),
                    Ordre = dto.Ordre,
                    DureeMinutes = dto.DureeMinutes
                };

                await _moduleRepo.CreateAsync(module);
                return CreatedAtAction(nameof(GetModules), new { formationId }, "Module créé.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur création module : {ex.Message}");
            }
        }

        
        // PUT /api/Formation/module/{id}
        
        [HttpPut("module/{id}")]
        public async Task<ActionResult> UpdateModule(int id, [FromBody] ModuleUpdateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Titre))
                    return BadRequest("Titre obligatoire.");

                var module = await _moduleRepo.GetByIdAsync(id);
                if (module == null)
                    return NotFound("Module introuvable.");

                var formation = await _formationRepo.GetByIdAsync(module.FormationId);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var userInfo = GetCurrentUser();
                if (userInfo.RoleId == 2 && formation.CreateurId != userInfo.UserId)
                    return StatusCode(403, "Vos formations seulement.");

                module.Titre = dto.Titre.Trim();
                module.Description = dto.Description?.Trim();
                module.Ordre = dto.Ordre;
                module.DureeMinutes = dto.DureeMinutes;

                await _moduleRepo.UpdateAsync(module);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur mise à jour module #{id} : {ex.Message}");
            }
        }

        
        // DELETE /api/Formation/module/{id}
        
        [HttpDelete("module/{id}")]
        public async Task<ActionResult> DeleteModule(int id)
        {
            try
            {
                var module = await _moduleRepo.GetByIdAsync(id);
                if (module == null)
                    return NotFound("Module introuvable.");

                var formation = await _formationRepo.GetByIdAsync(module.FormationId);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var userInfo = GetCurrentUser();
                if (userInfo.RoleId == 2 && formation.CreateurId != userInfo.UserId)
                    return StatusCode(403, "Vos formations seulement.");

                await _moduleRepo.DeleteAsync(id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur suppression module #{id} : {ex.Message}");
            }
        }

        
        // Helpers privés
        

        //Retourne utilisateur JWT. Lance exception si absent.</summary>
        private (int UserId, int RoleId) GetCurrentUser()
        {
            var result = TryGetCurrentUser();
            return result ?? throw new UnauthorizedAccessException("Token manquant/invalide.");
        }

        //Version nullable (AllowAnonymous). null si non connecté.</summary>
        private (int UserId, int RoleId)? TryGetCurrentUser()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (idClaim?.Value == null || roleClaim?.Value == null) return null;

            return (int.Parse(idClaim.Value), int.Parse(roleClaim.Value));
        }

        //Mapping Formation → DTO</summary>
        private static FormationReadDto MapToDto(Formation f) => new()
        {
            Id = f.Id,
            Titre = f.Titre,
            Description = f.Description,
            DateCreation = f.DateCreation,
            CreateurId = f.CreateurId,
            MediaType = f.MediaType,
            ModeDiffusion = f.ModeDiffusion,
            Langue = f.Langue,
            Niveau = f.Niveau,
            Prerequis = f.Prerequis,
            ImageUrl = f.ImageUrl,
            DureeMinutes = f.DureeMinutes,
            EstPublique = f.EstPublique
        };
    }
}
