using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    // Seul l'Admin (RoleId = 1) peut accéder à ce controller
    [Authorize(Roles = "1")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        
        // GET : api/role
        // Récupère tous les rôles
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetAll()
        {
            var roles = await _roleRepository.GetAllAsync();
            return Ok(roles);
        }

        
        // GET : api/role/id/{id}
        // Récupère un rôle par son ID
        
        [HttpGet("id/{id}")]
        public async Task<ActionResult<Role>> GetById(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
                return NotFound($"Aucun rôle trouvé avec l'ID : {id}");

            return Ok(role);
        }

        
        // GET : api/role/{name}
        // Récupère un rôle par son nom
        
        [HttpGet("{name}")]
        public async Task<ActionResult<Role>> GetByName(string name)
        {
            var role = await _roleRepository.GetByNameAsync(name);

            if (role == null)
                return NotFound($"Aucun rôle trouvé avec le nom : {name}");

            return Ok(role);
        }

        
        // POST : api/role
        // Crée un nouveau rôle
        
        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            await _roleRepository.CreateAsync(role);

            return Ok(new
            {
                message = "Rôle créé avec succès",
                role
            });
        }

        
        // DELETE : api/role/{id}
        // Supprime un rôle par ID
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null)
                return NotFound("Rôle introuvable");

            await _roleRepository.DeleteAsync(id);

            return Ok(new { message = "Rôle supprimé" });
        }
    }
}
