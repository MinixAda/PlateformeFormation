
// API/Controllers/FormationPrerequisController.cs
//
// Gère les prérequis entre formations.
// Exigence TFE : "vérification automatique des prérequis".
//
// Endpoints :
//   GET    /api/FormationPrerequis/{formationId}                → liste des prérequis
//   POST   /api/FormationPrerequis/{formationId}                → ajouter un prérequis (Admin)
//   DELETE /api/FormationPrerequis/{formationId}/{requiseId}    → supprimer (Admin)


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormationPrerequisController : ControllerBase
    {
        private readonly IFormationPrerequisRepository _prerequisRepo;
        private readonly IFormationRepository _formationRepo;

        public FormationPrerequisController(
            IFormationPrerequisRepository prerequisRepo,
            IFormationRepository formationRepo)
        {
            _prerequisRepo = prerequisRepo;
            _formationRepo = formationRepo;
        }

        
        // GET /api/FormationPrerequis/{formationId}
        
        //
        // Retourne la liste des prérequis d'une formation.
        // Public — utilisé par le frontend pour afficher les prérequis
        // sur la page de détail d'une formation.
        //
        [HttpGet("{formationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FormationPrerequisReadDto>>> GetPrerequis(
            int formationId)
        {
            try
            {
                // Vérifier que la formation existe
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound($"Formation #{formationId} introuvable.");

                var prerequis = await _prerequisRepo.GetPrerequisAsync(formationId);

                var result = prerequis.Select(p => new FormationPrerequisReadDto
                {
                    FormationId = p.FormationId,
                    FormationRequiseId = p.FormationRequiseId
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des prérequis de la formation #{formationId} : {ex.Message}");
            }
        }

        
        // POST /api/FormationPrerequis/{formationId}
        
        //
        // Ajoute un prérequis à une formation.
        // Réservé aux administrateurs.
        //
        // Vérifications :
        //   1. La formation cible existe
        //   2. La formation requise existe
        //   3. La formation ne se désigne pas elle-même comme prérequis
        //   4. Le lien n'existe pas déjà
        //
        [HttpPost("{formationId}")]
        [Authorize(Roles = "1")]  // Admin uniquement
        public async Task<ActionResult> AddPrerequis(
            int formationId,
            [FromBody] FormationPrerequisCreateDto dto)
        {
            try
            {
                // Vérification : pas de boucle sur soi-même
                if (formationId == dto.FormationRequiseId)
                    return BadRequest("Une formation ne peut pas être son propre prérequis.");

                // Vérifier que la formation cible existe
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return BadRequest($"La formation cible #{formationId} n'existe pas.");

                // Vérifier que la formation prérequise existe
                var formationRequise = await _formationRepo.GetByIdAsync(dto.FormationRequiseId);
                if (formationRequise == null)
                    return BadRequest($"La formation prérequise #{dto.FormationRequiseId} n'existe pas.");

                // Ajouter le prérequis (retourne false si déjà existant)
                bool ajoute = await _prerequisRepo.AddPrerequisAsync(formationId, dto.FormationRequiseId);
                if (!ajoute)
                    return BadRequest(
                        $"La formation « {formationRequise.Titre} » est déjà un prérequis " +
                        $"de la formation « {formation.Titre} ».");

                return Ok(
                    $"Prérequis ajouté : « {formationRequise.Titre} » est maintenant requise " +
                    $"avant « {formation.Titre} ».");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de l'ajout du prérequis : {ex.Message}");
            }
        }

        
        // DELETE /api/FormationPrerequis/{formationId}/{formationRequiseId}
        
        //
        // Supprime un prérequis entre deux formations.
        // Réservé aux administrateurs.
        //
        [HttpDelete("{formationId}/{formationRequiseId}")]
        [Authorize(Roles = "1")]  // Admin uniquement
        public async Task<ActionResult> RemovePrerequis(int formationId, int formationRequiseId)
        {
            try
            {
                bool supprime = await _prerequisRepo.RemovePrerequisAsync(formationId, formationRequiseId);
                if (!supprime)
                    return BadRequest(
                        $"Aucun prérequis trouvé entre la formation #{formationId} " +
                        $"et la formation #{formationRequiseId}.");

                return Ok("Prérequis supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la suppression du prérequis : {ex.Message}");
            }
        }
    }
}