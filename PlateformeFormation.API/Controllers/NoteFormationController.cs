
// PlateformeFormation.API/Controllers/NoteFormationController.cs
//
// FICHIER MANQUANT — à créer dans le projet.
//
// Responsabilités :
//   POST /api/NoteFormation              → noter une formation (apprenant inscrit)
//   GET  /api/NoteFormation/{formationId} → résumé note moyenne + count
//   GET  /api/NoteFormation/{formationId}/ma-note → note de l'utilisateur connecté
//
// Exigence TFE :
//   "Noter (4.5/5) une formation"


using System;
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
    [Authorize]
    public class NoteFormationController : ControllerBase
    {
        private readonly INoteFormationRepository _noteRepo;
        private readonly IFormationRepository _formationRepo;
        private readonly IInscriptionRepository _inscriptionRepo;

        public NoteFormationController(
            INoteFormationRepository noteRepo,
            IFormationRepository formationRepo,
            IInscriptionRepository inscriptionRepo)
        {
            _noteRepo = noteRepo
                ?? throw new ArgumentNullException(nameof(noteRepo));
            _formationRepo = formationRepo
                ?? throw new ArgumentNullException(nameof(formationRepo));
            _inscriptionRepo = inscriptionRepo
                ?? throw new ArgumentNullException(nameof(inscriptionRepo));
        }

        
        // POST /api/NoteFormation
        
        //
        // Crée ou met à jour la note d'un apprenant pour une formation.
        // UPSERT : si l'apprenant a déjà noté, sa note est mise à jour.
        //
        // Règles de validation :
        //   1. La formation doit exister.
        //   2. L'apprenant doit être inscrit à la formation.
        //   3. La note doit être entre 0.5 et 5.0.
        //
        // Note : on autorise la notation même si la formation n'est pas
        // terminée (les consignes ne précisent pas cette contrainte).
        // Adapter si nécessaire.
        //
        // Exigence TFE : "Noter (4.5/5) une formation"
        //
        [HttpPost]
        public async Task<ActionResult> Noter([FromBody] NoteFormationCreateDto dto)
        {
            try
            {
                int userId = GetUserId();

                // 1) La formation existe ?
                var formation = await _formationRepo.GetByIdAsync(dto.FormationId);
                if (formation == null)
                    return NotFound($"Formation #{dto.FormationId} introuvable.");

                // 2) L'apprenant est inscrit ?
                bool estInscrit = await _inscriptionRepo
                    .IsAlreadyInscribedAsync(userId, dto.FormationId);

                if (!estInscrit)
                    return BadRequest(
                        "Vous devez être inscrit à la formation pour la noter.");

                // 3) Validation de la note (double vérification en plus de [Range])
                if (dto.Note < 0.5m || dto.Note > 5.0m)
                    return BadRequest("La note doit être comprise entre 0.5 et 5.0.");

                // UPSERT via MERGE SQL (dans NoteFormationRepository.UpsertNoteAsync)
                await _noteRepo.UpsertNoteAsync(new NoteFormation
                {
                    UtilisateurId = userId,
                    FormationId = dto.FormationId,
                    Note = dto.Note
                    // DateNote assignée par SQL (GETDATE())
                });

                return Ok(
                    $"Note de {dto.Note}/5 enregistrée pour la formation " +
                    $"« {formation.Titre} ».");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de l'enregistrement de la note : {ex.Message}");
            }
        }

        
        // GET /api/NoteFormation/{formationId}
        
        //
        // Retourne le résumé des notes d'une formation :
        //   - Moyenne (arrondie à 1 décimale)
        //   - Nombre total de notes
        //
        // Public — affiché sur les pages de détail et de liste des formations.
        // Retourne moyenne = null si aucune note n'a encore été soumise.
        //
        [HttpGet("{formationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<NoteFormationResumeDto>> GetResume(int formationId)
        {
            try
            {
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound($"Formation #{formationId} introuvable.");

                decimal? moyenne = await _noteRepo.GetMoyenneAsync(formationId);
                int count = await _noteRepo.CountNotesAsync(formationId);

                return Ok(new NoteFormationResumeDto
                {
                    FormationId = formationId,
                    Moyenne = moyenne.HasValue
                        ? Math.Round(moyenne.Value, 1)
                        : null,
                    NombreNotes = count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des notes " +
                    $"(Formation #{formationId}) : {ex.Message}");
            }
        }

        
        // GET /api/NoteFormation/{formationId}/ma-note
        
        //
        // Retourne la note de l'utilisateur connecté pour une formation.
        // Permet au frontend d'afficher la note actuelle dans l'interface.
        // Retourne 404 si l'utilisateur n'a pas encore noté cette formation.
        //
        [HttpGet("{formationId}/ma-note")]
        public async Task<ActionResult> GetMaNote(int formationId)
        {
            try
            {
                int userId = GetUserId();

                var note = await _noteRepo.GetNoteUtilisateurAsync(userId, formationId);

                if (note == null)
                    return NotFound("Vous n'avez pas encore noté cette formation.");

                return Ok(new
                {
                    FormationId = note.FormationId,
                    Note = note.Note,
                    DateNote = note.DateNote
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération de votre note : {ex.Message}");
            }
        }

        
        // Helper privé
        

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "Token invalide : claim NameIdentifier manquant.");
            return int.Parse(claim.Value);
        }
    }
}
