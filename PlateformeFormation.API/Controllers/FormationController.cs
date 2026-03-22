using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Controller gérant les opérations CRUD sur les formations et leurs modules.
    // Accessible aux administrateurs (1) et formateurs (2).
    // Les GET sont publics.
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1,2")] // Admin (1) + Formateur (2)
    public class FormationController : ControllerBase
    {
        private readonly IFormationRepository _formationRepo;
        private readonly IModuleRepository _moduleRepo;

        public FormationController(IFormationRepository formationRepo, IModuleRepository moduleRepo)
        {
            _formationRepo = formationRepo;
            _moduleRepo = moduleRepo;
        }

     
        // GET : Récupérer toutes les formations (public)
 
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FormationReadDto>>> GetAll()
        {
            try
            {
                var formations = await _formationRepo.GetAllAsync();

                var result = formations.Select(f => new FormationReadDto
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
                    DureeMinutes = f.DureeMinutes
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des formations : {ex.Message}");
            }
        }

       
        // GET : Récupérer une formation par ID (public)
       
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FormationReadDto>> GetById(int id)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var dto = new FormationReadDto
                {
                    Id = formation.Id,
                    Titre = formation.Titre,
                    Description = formation.Description,
                    DateCreation = formation.DateCreation,
                    CreateurId = formation.CreateurId,
                    MediaType = formation.MediaType,
                    ModeDiffusion = formation.ModeDiffusion,
                    Langue = formation.Langue,
                    Niveau = formation.Niveau,
                    Prerequis = formation.Prerequis,
                    ImageUrl = formation.ImageUrl,
                    DureeMinutes = formation.DureeMinutes
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de la formation : {ex.Message}");
            }
        }

       
        // POST : Créer une formation
       
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FormationCreateDto dto)
        {
            try
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim == null)
                    return Unauthorized("Impossible de déterminer l'utilisateur connecté.");

                int createurId = int.Parse(idClaim.Value);

                var formation = new Formation
                {
                    Titre = dto.Titre,
                    Description = dto.Description,
                    DateCreation = DateTime.Now,
                    CreateurId = createurId,
                    MediaType = dto.MediaType,
                    ModeDiffusion = dto.ModeDiffusion,
                    Langue = dto.Langue,
                    Niveau = dto.Niveau,
                    Prerequis = dto.Prerequis,
                    ImageUrl = dto.ImageUrl,
                    DureeMinutes = dto.DureeMinutes
                };

                await _formationRepo.CreateAsync(formation);
                return Ok("Formation créée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la formation : {ex.Message}");
            }
        }

       
        // PUT : Mettre à jour une formation
       
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] FormationUpdateDto dto)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var user = GetCurrentUser();

                if (user.RoleId == 2 && formation.CreateurId != user.UserId)
                    return Forbid("Vous ne pouvez modifier que vos propres formations.");

                formation.Titre = dto.Titre;
                formation.Description = dto.Description;
                formation.MediaType = dto.MediaType;
                formation.ModeDiffusion = dto.ModeDiffusion;
                formation.Langue = dto.Langue;
                formation.Niveau = dto.Niveau;
                formation.Prerequis = dto.Prerequis;
                formation.ImageUrl = dto.ImageUrl;
                formation.DureeMinutes = dto.DureeMinutes;

                await _formationRepo.UpdateAsync(formation);
                return Ok("Formation mise à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de la formation : {ex.Message}");
            }
        }

       
        // DELETE : Supprimer une formation
       
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var user = GetCurrentUser();

                if (user.RoleId == 2 && formation.CreateurId != user.UserId)
                    return Forbid("Vous ne pouvez supprimer que vos propres formations.");

                await _formationRepo.DeleteAsync(id);
                return Ok("Formation supprimée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la formation : {ex.Message}");
            }
        }

        // ========
        // MODULES
        // ========

       
        // GET : Récupérer les modules d'une formation
       
        [HttpGet("{formationId}/modules")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Module>>> GetModules(int formationId)
        {
            try
            {
                var modules = await _moduleRepo.GetByFormationIdAsync(formationId);
                return Ok(modules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des modules : {ex.Message}");
            }
        }

       
        // POST : Ajouter un module à une formation
       
        [HttpPost("{formationId}/modules")]
        public async Task<ActionResult> CreateModule(int formationId, [FromBody] ModuleCreateDto dto)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var user = GetCurrentUser();

                if (user.RoleId == 2 && formation.CreateurId != user.UserId)
                    return Forbid("Vous ne pouvez ajouter des modules qu'à vos propres formations.");

                var module = new Module
                {
                    FormationId = formationId,
                    Titre = dto.Titre,
                    Description = dto.Description,
                    Ordre = dto.Ordre,
                    DureeMinutes = dto.DureeMinutes
                };

                await _moduleRepo.CreateAsync(module);
                return Ok("Module créé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création du module : {ex.Message}");
            }
        }

       
        // PUT : Modifier un module
       
        [HttpPut("module/{id}")]
        public async Task<ActionResult> UpdateModule(int id, [FromBody] ModuleUpdateDto dto)
        {
            try
            {
                var module = await _moduleRepo.GetByIdAsync(id);
                if (module == null)
                    return NotFound("Module introuvable.");

                var formation = await _formationRepo.GetByIdAsync(module.FormationId);
                var user = GetCurrentUser();

                if (user.RoleId == 2 && formation!.CreateurId != user.UserId)
                    return Forbid("Vous ne pouvez modifier que les modules de vos propres formations.");

                module.Titre = dto.Titre;
                module.Description = dto.Description;
                module.Ordre = dto.Ordre;
                module.DureeMinutes = dto.DureeMinutes;

                await _moduleRepo.UpdateAsync(module);
                return Ok("Module mis à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour du module : {ex.Message}");
            }
        }

       
        // DELETE : Supprimer un module
       
        [HttpDelete("module/{id}")]
        public async Task<ActionResult> DeleteModule(int id)
        {
            try
            {
                var module = await _moduleRepo.GetByIdAsync(id);
                if (module == null)
                    return NotFound("Module introuvable.");

                var formation = await _formationRepo.GetByIdAsync(module.FormationId);
                var user = GetCurrentUser();

                if (user.RoleId == 2 && formation!.CreateurId != user.UserId)
                    return Forbid("Vous ne pouvez supprimer que les modules de vos propres formations.");

                await _moduleRepo.DeleteAsync(id);
                return Ok("Module supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression du module : {ex.Message}");
            }
        }

       
        // Méthode privée : récupère l'utilisateur connecté + son rôle
       
        private (int UserId, int RoleId) GetCurrentUser()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirst(ClaimTypes.Role);

            if (idClaim == null || roleClaim == null)
                throw new Exception("Impossible de déterminer l'utilisateur connecté ou son rôle.");

            int userId = int.Parse(idClaim.Value);
            int roleId = int.Parse(roleClaim.Value);

            return (userId, roleId);
        }
    }
}
