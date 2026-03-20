using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos.Prerequis;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Controller gérant les prérequis entre formations.
    
    [Route("api/[controller]")]
    [ApiController]
    public class FormationPrerequisController : ControllerBase
    {
        private readonly IFormationPrerequisRepository _repo;
        private readonly IFormationRepository _formationRepo;

        public FormationPrerequisController(
            IFormationPrerequisRepository repo,
            IFormationRepository formationRepo)
        {
            _repo = repo;
            _formationRepo = formationRepo;
        }

        
        // Récupère les prérequis d'une formation.
        
        [HttpGet("{formationId}")]
        public async Task<ActionResult<IEnumerable<FormationPrerequisReadDto>>> GetPrerequis(int formationId)
        {
            var prerequis = await _repo.GetPrerequisAsync(formationId);

            var result = prerequis.Select(p => new FormationPrerequisReadDto
            {
                FormationId = p.FormationId,
                FormationRequiseId = p.FormationRequiseId
            });

            return Ok(result);
        }

        
        // Ajoute un prérequis à une formation (Admin uniquement).
        
        [HttpPost("{formationId}")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult> AddPrerequis(int formationId, [FromBody] FormationPrerequisCreateDto dto)
        {
            // Vérifier que les formations existent
            if (await _formationRepo.GetByIdAsync(formationId) == null)
                return BadRequest("La formation cible n'existe pas.");

            if (await _formationRepo.GetByIdAsync(dto.FormationRequiseId) == null)
                return BadRequest("La formation prérequise n'existe pas.");

            var added = await _repo.AddPrerequisAsync(formationId, dto.FormationRequiseId);

            if (!added)
                return BadRequest("Ce prérequis existe déjà pour cette formation.");

            return Ok("Prérequis ajouté avec succès.");
        }

        
        // Supprime un prérequis (Admin uniquement).
        
        [HttpDelete("{formationId}/{formationRequiseId}")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult> RemovePrerequis(int formationId, int formationRequiseId)
        {
            var removed = await _repo.RemovePrerequisAsync(formationId, formationRequiseId);

            if (!removed)
                return BadRequest("Impossible de supprimer : ce prérequis n'existe pas pour cette formation.");

            return Ok("Prérequis supprimé avec succès.");
        }
    }
}
