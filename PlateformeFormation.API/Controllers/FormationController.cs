using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using System.Security.Claims;

namespace PlateformeFormation.API.Controllers
{
    
    // Controller gérant toutes les opérations CRUD sur les formations.
    
    [Route("api/[controller]")]
    [ApiController]
    public class FormationController : ControllerBase
    {
        private readonly IFormationRepository _repo;

        public FormationController(IFormationRepository repo)
        {
            _repo = repo;
        }

        
        // Récupère toutes les formations.
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormationReadDto>>> GetAll()
        {
            var formations = await _repo.GetAllAsync();

            return Ok(formations.Select(f => new FormationReadDto
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
            }));
        }

        
        // Récupère une formation par ID.
        
        [HttpGet("{id}")]
        public async Task<ActionResult<FormationReadDto>> GetById(int id)
        {
            var f = await _repo.GetByIdAsync(id);
            if (f == null) return NotFound();

            return Ok(new FormationReadDto
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
        }

        
        // Crée une nouvelle formation (Formateur ou Admin uniquement).
        
        [HttpPost]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult> Create(FormationCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var formation = new Formation
            {
                Titre = dto.Titre,
                Description = dto.Description,
                DateCreation = DateTime.Now,
                CreateurId = userId,
                MediaType = dto.MediaType,
                ModeDiffusion = dto.ModeDiffusion,
                Langue = dto.Langue,
                Niveau = dto.Niveau,
                Prerequis = dto.Prerequis,
                ImageUrl = dto.ImageUrl,
                DureeMinutes = dto.DureeMinutes
            };

            await _repo.CreateAsync(formation);
            return Ok("Formation créée.");
        }

        
        // Met à jour une formation existante.
        
        [HttpPut("{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult> Update(int id, FormationUpdateDto dto)
        {
            var formation = await _repo.GetByIdAsync(id);
            if (formation == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var roleId = int.Parse(User.FindFirst(ClaimTypes.Role)!.Value);

            // Un formateur ne peut modifier que ses propres formations
            if (roleId == 2 && formation.CreateurId != userId)
                return Forbid();

            formation.Titre = dto.Titre;
            formation.Description = dto.Description;
            formation.MediaType = dto.MediaType;
            formation.ModeDiffusion = dto.ModeDiffusion;
            formation.Langue = dto.Langue;
            formation.Niveau = dto.Niveau;
            formation.Prerequis = dto.Prerequis;
            formation.ImageUrl = dto.ImageUrl;
            formation.DureeMinutes = dto.DureeMinutes;

            await _repo.UpdateAsync(formation);
            return Ok("Formation mise à jour.");
        }

        
        // Supprime une formation (Admin uniquement).
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult> Delete(int id)
        {
            var formation = await _repo.GetByIdAsync(id);
            if (formation == null) return NotFound();

            await _repo.DeleteAsync(id);
            return Ok("Formation supprimée.");
        }
    }
}
