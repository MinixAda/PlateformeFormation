using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos.Prerequis;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Gère les prérequis entre formations.
    // Permet d'ajouter, retirer et consulter les prérequis.
    
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

        
        // GET : Récupérer les prérequis d'une formation
        
        
        // Récupère la liste des prérequis d'une formation donnée.
        
        [HttpGet("{formationId}")]
        public async Task<ActionResult<IEnumerable<FormationPrerequisReadDto>>> GetPrerequis(int formationId)
        {
            try
            {
                // Vérifier que la formation existe
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                var prerequis = await _repo.GetPrerequisAsync(formationId);

                var result = prerequis.Select(p => new FormationPrerequisReadDto
                {
                    FormationId = p.FormationId,
                    FormationRequiseId = p.FormationRequiseId
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des prérequis : {ex.Message}");
            }
        }

        
        // POST : Ajouter un prérequis
        
        
        // Ajoute un prérequis à une formation.
        // Accessible uniquement aux administrateurs.
        
        [HttpPost("{formationId}")]
        [Authorize(Roles = "1")] // Admin uniquement
        public async Task<ActionResult> AddPrerequis(int formationId, [FromBody] FormationPrerequisCreateDto dto)
        {
            try
            {
                // Vérifier que la formation cible existe
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return BadRequest("La formation cible n'existe pas.");

                // Vérifier que la formation prérequise existe
                var formationRequise = await _formationRepo.GetByIdAsync(dto.FormationRequiseId);
                if (formationRequise == null)
                    return BadRequest("La formation prérequise n'existe pas.");

                // Ajouter le prérequis
                var added = await _repo.AddPrerequisAsync(formationId, dto.FormationRequiseId);

                if (!added)
                    return BadRequest("Ce prérequis existe déjà.");

                return Ok("Prérequis ajouté avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'ajout du prérequis : {ex.Message}");
            }
        }

        
        // DELETE : Supprimer un prérequis
        
        
        // Supprime un prérequis entre deux formations.
        // Accessible uniquement aux administrateurs.
        
        [HttpDelete("{formationId}/{formationRequiseId}")]
        [Authorize(Roles = "1")] // Admin uniquement
        public async Task<ActionResult> RemovePrerequis(int formationId, int formationRequiseId)
        {
            try
            {
                var removed = await _repo.RemovePrerequisAsync(formationId, formationRequiseId);

                if (!removed)
                    return BadRequest("Ce prérequis n'existe pas.");

                return Ok("Prérequis supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression du prérequis : {ex.Message}");
            }
        }
    }
}
