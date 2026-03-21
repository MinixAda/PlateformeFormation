using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Gère les opérations CRUD sur les formations
    // Accessible aux administrateurs et formateurs
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1,2")] // Admin (1) + Formateur (2)
    public class FormationController : ControllerBase
    {
        private readonly IFormationRepository _repo;

        
        // Constructeur avec injection du repository de formation.
        
        public FormationController(IFormationRepository repo)
        {
            _repo = repo;
        }

        
        // Get : Récupérer toutes les formations
        
        
        // Récupère la liste de toutes les formations.
        // Accessible anonymement.
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FormationReadDto>>> GetAll()
        {
            try
            {
                var formations = await _repo.GetAllAsync();

                // Mapping entité -> DTO
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
                // Erreur serveur générique avec message explicite
                return StatusCode(500, $"Erreur lors de la récupération des formations : {ex.Message}");
            }
        }

        
        // Get : Récupérer une formation par ID
               
        // Récupère une formation par son identifiant.
        // Accessible anonymement.
        
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FormationReadDto>> GetById(int id)
        {
            try
            {
                var formation = await _repo.GetByIdAsync(id);
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

        
        // Post : Créer une formation
               
        // Crée une nouvelle formation
        // Le créateur est l'utilisateur connecté (ID dans le JWT)
        
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] FormationCreateDto dto)
        {
            try
            {
                // Récupérer l'ID du créateur depuis le JWT
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim == null)
                    return Unauthorized("Impossible de déterminer l'utilisateur connecté.");

                int createurId = int.Parse(idClaim.Value);

                var formation = new Formation
                {
                    Titre = dto.Titre,
                    Description = dto.Description,
                    DateCreation = DateTime.Now,
                    CreateurId = createurId, //  plus de dto.CreateurId
                    MediaType = dto.MediaType,
                    ModeDiffusion = dto.ModeDiffusion,
                    Langue = dto.Langue,
                    Niveau = dto.Niveau,
                    Prerequis = dto.Prerequis,
                    ImageUrl = dto.ImageUrl,
                    DureeMinutes = dto.DureeMinutes
                };

                await _repo.CreateAsync(formation);
                return Ok("Formation créée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la création de la formation : {ex.Message}");
            }
        }

        
        // Put : Mettre à jour une formation
               
        // Met à jour une formation existante
        
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] FormationUpdateDto dto)
        {
            try
            {
                var formation = await _repo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                // Mise à jour des champs modifiables
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
                return Ok("Formation mise à jour avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la mise à jour de la formation : {ex.Message}");
            }
        }

        
        // Delete: Supprimer une formation
               
        // Supprime une formation par son identifiant.
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var formation = await _repo.GetByIdAsync(id);
                if (formation == null)
                    return NotFound("Formation introuvable.");

                await _repo.DeleteAsync(id);
                return Ok("Formation supprimée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la suppression de la formation : {ex.Message}");
            }
        }
    }
}
