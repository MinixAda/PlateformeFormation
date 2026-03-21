using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos.Progression;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Gère la progression des utilisateurs sur les modules.
    // Permet de marquer un module comme terminé et de consulter la progression.
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // L'utilisateur doit être connecté
    public class ModuleProgressionController : ControllerBase
    {
        private readonly IModuleProgressionRepository _progressionRepo;
        private readonly IFormationRepository _formationRepo;
        private readonly IInscriptionRepository _inscriptionRepo;

        public ModuleProgressionController(
            IModuleProgressionRepository progressionRepo,
            IFormationRepository formationRepo,
            IInscriptionRepository inscriptionRepo)
        {
            _progressionRepo = progressionRepo;
            _formationRepo = formationRepo;
            _inscriptionRepo = inscriptionRepo;
        }

        
        // POST : Marquer un module comme terminé
        
        
        // Marque un module comme terminé pour l'utilisateur connecté.
        
        [HttpPost("terminer")]
        public async Task<ActionResult> TerminerModule([FromBody] CompleteModuleDto dto)
        {
            try
            {
                // Récupérer l'ID utilisateur depuis le JWT
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Vérifier que le module existe
                var module = await _formationRepo.GetModuleByIdAsync(dto.ModuleId);
                if (module == null)
                    return BadRequest("Le module demandé n'existe pas.");

                // Vérifier que l'utilisateur est inscrit à la formation
                if (!await _inscriptionRepo.IsAlreadyInscribedAsync(userId, module.FormationId))
                    return BadRequest("Vous devez être inscrit à la formation pour valider un module.");

                // Vérifier si le module est déjà terminé
                if (await _progressionRepo.IsModuleCompletedAsync(userId, dto.ModuleId))
                    return BadRequest("Ce module est déjà terminé.");

                // Marquer le module comme terminé
                await _progressionRepo.CompleteModuleAsync(userId, dto.ModuleId);

                // Vérifier si tous les modules de la formation sont terminés
                bool allDone = await _progressionRepo.HasCompletedAllModulesAsync(userId, module.FormationId);

                if (allDone)
                {
                    await _inscriptionRepo.MarkAsCompletedAsync(userId, module.FormationId);
                    return Ok("Module terminé. Tous les modules sont complétés : la formation est maintenant terminée.");
                }

                return Ok("Module terminé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la complétion du module : {ex.Message}");
            }
        }

        
        // GET : Récupérer la progression sur une formation
        
        
        // Récupère la progression de l'utilisateur connecté sur une formation.
        
        [HttpGet("formation/{formationId}")]
        public async Task<ActionResult<IEnumerable<ModuleProgressionReadDto>>> GetProgression(int formationId)
        {
            try
            {
                // ID utilisateur depuis le JWT
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Vérifier que la formation existe
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return BadRequest("La formation demandée n'existe pas.");

                // Récupérer la progression
                var progression = await _progressionRepo.GetProgressionAsync(userId, formationId);

                var result = progression.Select(p => new ModuleProgressionReadDto
                {
                    ModuleId = p.ModuleId,
                    EstTermine = p.EstTermine,
                    DateCompletion = p.DateCompletion
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération de la progression : {ex.Message}");
            }
        }
    }
}
