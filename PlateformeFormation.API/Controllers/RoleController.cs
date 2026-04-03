
// API/Controllers/RoleController.cs
// Gère les rôles disponibles sur la plateforme.
// GET est public (utilisé par les formulaires de création).
// POST et DELETE sont réservés aux administrateurs.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepo;

        public RoleController(IRoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        
        // GET /api/Role            
        // Retourne la liste de tous les rôles disponibles.
        // Public — utilisé par les formulaires admin (select de rôle).
      
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RoleReadDto>>> GetAll()
        {
            try
            {
                var roles = await _roleRepo.GetAllAsync();
                var result = roles.Select(r => new RoleReadDto { Id = r.Id, Nom = r.Nom });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des rôles : {ex.Message}");
            }
        }

        
        // GET /api/Role/{id}
        
        // Retourne un rôle par son ID.
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RoleReadDto>> GetById(int id)
        {
            try
            {
                var role = await _roleRepo.GetByIdAsync(id);
                if (role == null)
                    return NotFound($"Rôle #{id} introuvable.");

                return Ok(new RoleReadDto { Id = role.Id, Nom = role.Nom });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération du rôle #{id} : {ex.Message}");
            }
        }

        
        // POST /api/Role
        
        //Crée un nouveau rôle. Réservé aux administrateurs.</summary>
        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Create([FromBody] RoleCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nom))
                    return BadRequest("Le nom du rôle est obligatoire.");

                // Vérifier l'unicité du nom
                var existing = await _roleRepo.GetByNameAsync(dto.Nom.Trim());
                if (existing != null)
                    return BadRequest($"Un rôle nommé « {dto.Nom} » existe déjà.");

                await _roleRepo.CreateAsync(new Role { Nom = dto.Nom.Trim() });
                return Ok($"Rôle « {dto.Nom} » créé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la création du rôle : {ex.Message}");
            }
        }

        
        // DELETE /api/Role/{id}              
        // Supprime un rôle par son ID.
        // Protection : les rôles système (1, 2, 3) ne peuvent pas être supprimés.
       
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // Protéger les rôles système fondamentaux
                if (id is 1 or 2 or 3)
                    return BadRequest(
                        "Les rôles système (Admin=1, Formateur=2, Apprenant=3) " +
                        "ne peuvent pas être supprimés.");

                var role = await _roleRepo.GetByIdAsync(id);
                if (role == null)
                    return NotFound($"Rôle #{id} introuvable.");

                await _roleRepo.DeleteAsync(id);
                return Ok($"Rôle « {role.Nom} » supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la suppression du rôle #{id} : {ex.Message}");
            }
        }
    }
}