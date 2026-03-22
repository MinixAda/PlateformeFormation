using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Contrôleur gérant les rôles (Admin, Formateur, Apprenant, etc.).
    
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _repo;

        public RoleController(IRoleRepository repo)
        {
            _repo = repo;
        }

        
        // Récupère la liste de tous les rôles.
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleReadDto>>> GetAll()
        {
            try
            {
                var roles = await _repo.GetAllAsync();

                var result = roles.Select(r => new RoleReadDto
                {
                    Id = r.Id,
                    Nom = r.Nom
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des rôles : {ex.Message}");
            }
        }

        
        // Récupère un rôle par son identifiant.
        
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleReadDto>> GetById(int id)
        {
            try
            {
                var role = await _repo.GetByIdAsync(id);
                if (role == null)
                    return NotFound("Rôle introuvable.");

                var dto = new RoleReadDto
                {
                    Id = role.Id,
                    Nom = role.Nom
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération du rôle : {ex.Message}");
            }
        }

        
        // Crée un nouveau rôle.
        
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] RoleCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nom))
                    return BadRequest("Le nom du rôle est obligatoire.");

                var role = new Role
                {
                    Nom = dto.Nom
                };

                await _repo.CreateAsync(role);
                return Ok("Rôle créé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la création du rôle : {ex.Message}");
            }
        }

        
        // Supprime un rôle par son identifiant.
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var role = await _repo.GetByIdAsync(id);
                if (role == null)
                    return NotFound("Rôle introuvable.");

                await _repo.DeleteAsync(id);
                return Ok("Rôle supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la suppression du rôle : {ex.Message}");
            }
        }
    }
}
