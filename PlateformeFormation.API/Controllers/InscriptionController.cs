
// API/Controllers/InscriptionController.cs
//
// CORRECTION CRITIQUE APPLIQUÉE :
//   Vérification des prérequis avant inscription — absente dans
//   l'original alors qu'elle est explicitement exigée dans les
//   consignes TFE ("vérification automatique des prérequis").
//
//   Pour chaque prérequis de la formation cible, on vérifie que
//   l'utilisateur a un statut "Terminé" sur cette formation requise.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // L'utilisateur doit être connecté pour toutes les actions
    public class InscriptionController : ControllerBase
    {
        private readonly IInscriptionRepository _inscriptionRepo;
        private readonly IFormationRepository _formationRepo;
        private readonly IFormationPrerequisRepository _prerequisRepo;

        public InscriptionController(
            IInscriptionRepository inscriptionRepo,
            IFormationRepository formationRepo,
            IFormationPrerequisRepository prerequisRepo)
        {
            _inscriptionRepo = inscriptionRepo;
            _formationRepo = formationRepo;
            _prerequisRepo = prerequisRepo;
        }

        
        // POST /api/Inscription
        
        //
        // Inscrit l'utilisateur connecté à une formation.
        //
        // Vérifications dans l'ordre :
        //   1. La formation existe
        //   2. L'utilisateur n'est pas déjà inscrit
        //   3. Tous les prérequis sont satisfaits (formation terminée)
        //      → CORRECTION : cette vérification était absente dans l'original
        //
        [HttpPost]
        public async Task<ActionResult> Inscrire([FromBody] InscriptionCreateDto dto)
        {
            try
            {
                // Extraire l'ID utilisateur depuis le JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

                // 1) Vérifier que la formation existe
                var formation = await _formationRepo.GetByIdAsync(dto.FormationId);
                if (formation == null)
                    return BadRequest("La formation demandée n'existe pas.");

                // 2) Vérifier si l'utilisateur est déjà inscrit
                if (await _inscriptionRepo.IsAlreadyInscribedAsync(userId, dto.FormationId))
                    return BadRequest("Vous êtes déjà inscrit à cette formation.");

                // 3) CORRECTION — Vérification automatique des prérequis
                //    Pour chaque prérequis de la formation, on vérifie que
                //    l'utilisateur a bien terminé la formation requise.
                var prerequis = await _prerequisRepo.GetPrerequisAsync(dto.FormationId);

                foreach (var p in prerequis)
                {
                    bool prereqSatisfait = await _inscriptionRepo
                        .HasCompletedFormationAsync(userId, p.FormationRequiseId);

                    if (!prereqSatisfait)
                    {
                        // Récupérer le titre de la formation manquante pour un message explicite
                        var formationRequise = await _formationRepo.GetByIdAsync(p.FormationRequiseId);
                        string titreRequis = formationRequise?.Titre ?? $"Formation #{p.FormationRequiseId}";

                        return BadRequest(
                            $"Prérequis non satisfait : vous devez d'abord terminer la formation « {titreRequis} ».");
                    }
                }

                // 4) Créer l'inscription
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

        
        // GET /api/Inscription/mes-inscriptions
        
        //
        // Retourne toutes les inscriptions de l'utilisateur connecté.
        // Le frontend charge ensuite les détails de chaque formation par ID.
        //
        [HttpGet("mes-inscriptions")]
        public async Task<ActionResult<IEnumerable<InscriptionReadDto>>> GetMesInscriptions()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);

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