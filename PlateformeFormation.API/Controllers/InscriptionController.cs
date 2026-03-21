using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos.Inscription;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    
    // Gère les inscriptions des utilisateurs aux formations.
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // L'utilisateur doit être connecté
    public class InscriptionController : ControllerBase
    {
        private readonly IInscriptionRepository _inscriptionRepo;
        private readonly IFormationRepository _formationRepo;

        public InscriptionController(
            IInscriptionRepository inscriptionRepo,
            IFormationRepository formationRepo)
        {
            _inscriptionRepo = inscriptionRepo;
            _formationRepo = formationRepo;
        }

        
        // POST : Inscrire l'utilisateur connecté à une formation
        
        
        // Inscrit l'utilisateur connecté à une formation.
        
        [HttpPost]
        public async Task<ActionResult> Inscrire([FromBody] InscriptionCreateDto dto)
        {
            try
            {
                // Récupérer l'ID utilisateur depuis le JWT
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Vérifier que la formation existe
                var formation = await _formationRepo.GetByIdAsync(dto.FormationId);
                if (formation == null)
                    return BadRequest("La formation demandée n'existe pas.");

                // Vérifier si l'utilisateur est déjà inscrit
                if (await _inscriptionRepo.IsAlreadyInscribedAsync(userId, dto.FormationId))
                    return BadRequest("Vous êtes déjà inscrit à cette formation.");

                // Créer l'inscription
                var inscription = new Inscription
                {
                    UtilisateurId = userId,
                    FormationId = dto.FormationId,
                    DateInscription = DateTime.Now,
                    Statut = "EnCours"
                };

                await _inscriptionRepo.CreateAsync(inscription);

                return Ok("Inscription effectuée avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'inscription : {ex.Message}");
            }
        }

        
        // GET : Récupérer les inscriptions de l'utilisateur connecté
        
        
        // Récupère toutes les inscriptions de l'utilisateur connecté.
        
        [HttpGet("mes-inscriptions")]
        public async Task<ActionResult<IEnumerable<InscriptionReadDto>>> GetMesInscriptions()
        {
            try
            {
                // ID utilisateur depuis le JWT
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var inscriptions = await _inscriptionRepo.GetByUserAsync(userId);

                var result = inscriptions.Select(i => new InscriptionReadDto
                {
                    Id = i.Id,
                    FormationId = i.FormationId,
                    Statut = i.Statut,
                    DateInscription = i.DateInscription
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de la récupération des inscriptions : {ex.Message}");
            }
        }
    }
}
