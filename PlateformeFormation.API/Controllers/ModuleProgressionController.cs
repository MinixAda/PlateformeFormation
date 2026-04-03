
// API/Controllers/ModuleProgressionController.cs
//
// CORRECTIONS APPLIQUÉES :
//   1. Utilise IModuleRepository (cohérent avec l'architecture)
//      au lieu de _formationRepo.GetModuleByIdAsync()
//   2. Messages d'erreur plus explicites
//   3. Gestion d'exceptions complète


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModuleProgressionController : ControllerBase
    {
        private readonly IModuleProgressionRepository _progressionRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly IInscriptionRepository _inscriptionRepo;

        public ModuleProgressionController(
            IModuleProgressionRepository progressionRepo,
            IModuleRepository moduleRepo,
            IInscriptionRepository inscriptionRepo)
        {
            _progressionRepo = progressionRepo;
            _moduleRepo = moduleRepo;
            _inscriptionRepo = inscriptionRepo;
        }

        
        // POST /api/ModuleProgression/terminer
        
        //
        // Marque un module comme terminé pour l'utilisateur connecté.
        //
        // Vérifications :
        //   1. Le module existe
        //   2. L'utilisateur est inscrit à la formation parente
        //   3. Le module n'est pas déjà terminé
        //
        // Si tous les modules de la formation sont terminés :
        //   → L'inscription passe automatiquement au statut "Terminé"
        //   → L'attestation devient disponible
        //
        [HttpPost("terminer")]
        public async Task<ActionResult> TerminerModule([FromBody] CompleteModuleDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

                // 1) Vérifier que le module existe via IModuleRepository
                var module = await _moduleRepo.GetByIdAsync(dto.ModuleId);
                if (module == null)
                    return BadRequest("Le module demandé n'existe pas.");

                // 2) Vérifier que l'utilisateur est inscrit à la formation parente
                bool estInscrit = await _inscriptionRepo
                    .IsAlreadyInscribedAsync(userId, module.FormationId);

                if (!estInscrit)
                    return BadRequest(
                        "Vous devez être inscrit à la formation pour valider un module.");

                // 3) Vérifier si le module n'est pas déjà terminé
                if (await _progressionRepo.IsModuleCompletedAsync(userId, dto.ModuleId))
                    return BadRequest("Ce module est déjà marqué comme terminé.");

                // 4) Enregistrer la complétion (avec DateCompletion = GETDATE())
                await _progressionRepo.CompleteModuleAsync(userId, dto.ModuleId);

                // 5) Vérifier si tous les modules de la formation sont terminés
                bool tousTermines = await _progressionRepo
                    .HasCompletedAllModulesAsync(userId, module.FormationId);

                if (tousTermines)
                {
                    // Passer l'inscription en statut "Terminé"
                    await _inscriptionRepo.MarkAsCompletedAsync(userId, module.FormationId);
                    return Ok(
                        "Module terminé. Félicitations — vous avez complété toute la formation ! " +
                        "Votre attestation est désormais disponible.");
                }

                return Ok("Module terminé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la validation du module : {ex.Message}");
            }
        }

        
        // GET /api/ModuleProgression/formation/{formationId}
        
        //
        // Retourne la progression de l'utilisateur connecté sur une formation.
        // Utilisé par ProgressionPage et AttestationPage côté frontend.
        //
        [HttpGet("formation/{formationId}")]
        public async Task<ActionResult<IEnumerable<ModuleProgressionReadDto>>> GetProgression(
            int formationId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

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
                return StatusCode(500,
                    $"Erreur lors de la récupération de la progression " +
                    $"(Formation #{formationId}) : {ex.Message}");
            }
        }
    }
}