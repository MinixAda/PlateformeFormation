
// PlateformeFormation.API/Controllers/QcmController.cs
//
// Controller QCM — VERSION FINALE
// Ce fichier REMPLACE l'existant QcmController.cs.
//
// DTO utilisé pour la soumission : SoumettreQcmDto (unique, fusionné)
// SoumissionQcmDto.cs doit être SUPPRIMÉ du projet.
//
// Endpoints :
//   GET    /api/Qcm/{moduleId}/questions    → Questions + réponses (inscrit requis)
//   POST   /api/Qcm/{moduleId}/questions    → Créer une question (Formateur, Admin)
//   PUT    /api/Qcm/questions/{id}          → Modifier une question (Formateur, Admin)
//   DELETE /api/Qcm/questions/{id}          → Supprimer une question (Formateur, Admin)
//   POST   /api/Qcm/{moduleId}/valider      → Soumettre + corriger le QCM (inscrit)
//   GET    /api/Qcm/{moduleId}/tentatives   → Historique des tentatives (inscrit)


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
using PlateformeFormation.Infrastructure.Services;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QcmController : ControllerBase
    {
        private readonly IQcmRepository _qcmRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly IInscriptionRepository _inscriptionRepo;
        private readonly IModuleProgressionRepository _progressionRepo;
        private readonly AttestationService _attestationService;

        //Seuil de réussite : 60 % de bonnes réponses.</summary>
        private const double SeuilReussite = 0.60;

        public QcmController(
            IQcmRepository qcmRepo,
            IModuleRepository moduleRepo,
            IInscriptionRepository inscriptionRepo,
            IModuleProgressionRepository progressionRepo,
            AttestationService attestationService)
        {
            _qcmRepo = qcmRepo ?? throw new ArgumentNullException(nameof(qcmRepo));
            _moduleRepo = moduleRepo ?? throw new ArgumentNullException(nameof(moduleRepo));
            _inscriptionRepo = inscriptionRepo ?? throw new ArgumentNullException(nameof(inscriptionRepo));
            _progressionRepo = progressionRepo ?? throw new ArgumentNullException(nameof(progressionRepo));
            _attestationService = attestationService ?? throw new ArgumentNullException(nameof(attestationService));
        }

        
        // GET /api/Qcm/{moduleId}/questions
        
        //
        // Retourne les questions d'un module avec leurs réponses possibles.
        // EstCorrecte est volontairement absent — la correction se fait côté serveur.
        // Apprenants : doivent être inscrits à la formation parente.
        // Formateurs et Admins : accès libre sans inscription.
        //
        [HttpGet("{moduleId}/questions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<QuestionReadDto>>> GetQuestions(int moduleId)
        {
            try
            {
                int userId = GetUserId();
                int roleId = GetRoleId();

                // 1) Module existe ?
                var module = await _moduleRepo.GetByIdAsync(moduleId);
                if (module == null)
                    return NotFound($"Module #{moduleId} introuvable.");

                // 2) Apprenants (rôle 3) : vérifier l'inscription
                if (roleId == 3 &&
                    !await _inscriptionRepo.IsAlreadyInscribedAsync(userId, module.FormationId))
                {
                    return StatusCode(403,
                        "Vous devez être inscrit à la formation pour accéder aux QCM de ce module.");
                }

                // 3) Charger questions + réponses en une requête (anti N+1)
                var qcmComplet = await _qcmRepo.GetQcmCompletByModuleIdAsync(moduleId);

                var result = qcmComplet
                    .OrderBy(q => q.Question.Ordre)
                    .Select(q => new QuestionReadDto
                    {
                        Id = q.Question.Id,
                        Ordre = q.Question.Ordre,
                        Texte = q.Question.Texte,
                        // EstCorrecte volontairement absent — ne jamais révéler avant soumission
                        Reponses = q.Reponses.Select(r => new ReponseReadDto
                        {
                            Id = r.Id,
                            Texte = r.Texte
                        }).ToList()
                    });

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des questions " +
                    $"(module #{moduleId}) : {ex.Message}");
            }
        }

        
        // POST /api/Qcm/{moduleId}/questions
        
        //
        // Crée une question avec ses réponses pour un module donné.
        // Réservé aux Formateurs et Admins.
        //
        // Règles :
        //   - Texte de la question obligatoire.
        //   - Au moins 2 réponses non vides.
        //   - Exactement 1 réponse correcte (EstCorrecte = true).
        //
        [HttpPost("{moduleId}/questions")]
        [Authorize(Roles = "Formateur,Admin")]
        public async Task<ActionResult> CreateQuestion(
            int moduleId,
            [FromBody] QuestionCreateDto dto)
        {
            try
            {
                var module = await _moduleRepo.GetByIdAsync(moduleId);
                if (module == null)
                    return NotFound($"Module #{moduleId} introuvable.");

                if (string.IsNullOrWhiteSpace(dto.Texte))
                    return BadRequest("Le texte de la question est obligatoire.");

                if (dto.Reponses == null || dto.Reponses.Count < 2)
                    return BadRequest("Une question doit avoir au moins 2 réponses.");

                var reponsesNonVides = dto.Reponses
                    .Where(r => !string.IsNullOrWhiteSpace(r.Texte))
                    .ToList();

                if (reponsesNonVides.Count < 2)
                    return BadRequest("Au moins 2 réponses avec un texte non vide sont requises.");

                int nbCorrectes = reponsesNonVides.Count(r => r.EstCorrecte);
                if (nbCorrectes == 0)
                    return BadRequest(
                        "Marquez exactement une réponse comme correcte (EstCorrecte = true).");
                if (nbCorrectes > 1)
                    return BadRequest(
                        $"Une seule réponse correcte est autorisée. " +
                        $"Vous en avez marqué {nbCorrectes}.");

                // Ordre automatique si non fourni
                int ordre = dto.Ordre > 0
                    ? dto.Ordre
                    : await _qcmRepo.CountQuestionsAsync(moduleId) + 1;

                int questionId = await _qcmRepo.CreateQuestionAsync(new Question
                {
                    ModuleId = moduleId,
                    Texte = dto.Texte.Trim(),
                    Ordre = ordre
                });

                foreach (var rep in reponsesNonVides)
                {
                    await _qcmRepo.CreateReponseAsync(new Reponse
                    {
                        QuestionId = questionId,
                        Texte = rep.Texte.Trim(),
                        EstCorrecte = rep.EstCorrecte
                    });
                }

                return Ok($"Question créée (ID : {questionId}, ordre : {ordre}).");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la création de la question " +
                    $"(module #{moduleId}) : {ex.Message}");
            }
        }

        
        // PUT /api/Qcm/questions/{id}
        
        //
        // Modifie le texte et/ou l'ordre d'une question.
        // Les réponses existantes restent inchangées.
        // Pour les modifier, supprimer la question et en créer une nouvelle.
        // Réservé aux Formateurs et Admins.
        //
        [HttpPut("questions/{id}")]
        [Authorize(Roles = "Formateur,Admin")]
        public async Task<ActionResult> UpdateQuestion(
            int id,
            [FromBody] QuestionUpdateDto dto)
        {
            try
            {
                var question = await _qcmRepo.GetQuestionByIdAsync(id);
                if (question == null)
                    return NotFound($"Question #{id} introuvable.");

                if (string.IsNullOrWhiteSpace(dto.Texte))
                    return BadRequest("Le texte de la question est obligatoire.");

                question.Texte = dto.Texte.Trim();
                if (dto.Ordre > 0) question.Ordre = dto.Ordre;

                await _qcmRepo.UpdateQuestionAsync(question);

                return Ok($"Question #{id} mise à jour.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la mise à jour de la question #{id} : {ex.Message}");
            }
        }

        
        // DELETE /api/Qcm/questions/{id}
        
        //
        // Supprime une question et toutes ses réponses (CASCADE SQL).
        // Réservé aux Formateurs et Admins.
        //
        [HttpDelete("questions/{id}")]
        [Authorize(Roles = "Formateur,Admin")]
        public async Task<ActionResult> DeleteQuestion(int id)
        {
            try
            {
                var question = await _qcmRepo.GetQuestionByIdAsync(id);
                if (question == null)
                    return NotFound($"Question #{id} introuvable.");

                // Les réponses liées sont supprimées en CASCADE (FK_Reponse_Question)
                await _qcmRepo.DeleteQuestionAsync(id);

                return Ok($"Question #{id} et ses réponses supprimées.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la suppression de la question #{id} : {ex.Message}");
            }
        }

        
        // POST /api/Qcm/{moduleId}/valider
        
        //
        // Reçoit les réponses via SoumettreQcmDto (DTO unique fusionné),
        // corrige côté serveur, enregistre la tentative,
        // et déclenche automatiquement :
        //   → passage de l'inscription à "Terminé" si tous les modules sont faits
        //   → création de l'attestation (idempotente)
        //
        // Retourne ResultatQcmCompletDto avec FormationTerminee = true si applicable.
        //
        [HttpPost("{moduleId}/valider")]
        [Authorize]
        public async Task<ActionResult<ResultatQcmCompletDto>> ValiderQcm(
            int moduleId,
            [FromBody] SoumettreQcmDto dto)
        {
            try
            {
                int userId = GetUserId();

                // 1) Module existe ?
                var module = await _moduleRepo.GetByIdAsync(moduleId);
                if (module == null)
                    return NotFound($"Module #{moduleId} introuvable.");

                // 2) Apprenant inscrit ?
                if (!await _inscriptionRepo.IsAlreadyInscribedAsync(userId, module.FormationId))
                    return StatusCode(403,
                        "Vous devez être inscrit à la formation pour passer le QCM.");

                // 3) Charger les questions du module
                var questions = (await _qcmRepo.GetQuestionsByModuleIdAsync(moduleId)).ToList();

                if (!questions.Any())
                    return BadRequest(
                        "Ce module ne contient aucune question. " +
                        "Le QCM ne peut pas être soumis.");

                // 4) Indexer les réponses reçues par questionId → O(1)
                var reponsesRecues = dto.Reponses.ToDictionary(
                    r => r.QuestionId,
                    r => r.ReponseId);

                // 5) Toutes les questions ont-elles une réponse ?
                var manquantes = questions
                    .Where(q => !reponsesRecues.ContainsKey(q.Id))
                    .ToList();

                if (manquantes.Any())
                    return BadRequest(
                        $"{manquantes.Count} question(s) sans réponse. " +
                        "Répondez à toutes les questions avant de valider.");

                // 6) Correction côté serveur — jamais côté client
                int score = 0;
                foreach (var question in questions)
                {
                    bool estCorrecte = await _qcmRepo.IsReponseCorrecteAsync(
                        question.Id, reponsesRecues[question.Id]);
                    if (estCorrecte) score++;
                }

                // 7) Calculer le résultat
                int total = questions.Count;
                bool reussi = (double)score / total >= SeuilReussite;
                int pourcentage = (int)Math.Round((double)score / total * 100);

                string message = reussi
                    ? $"Félicitations ! {score}/{total} ({pourcentage}%) — QCM réussi."
                    : $"Score : {score}/{total} ({pourcentage}%). " +
                      "Seuil de réussite : 60 %. Révisez et réessayez.";

                // 8) Enregistrer la tentative
                await _qcmRepo.CreateTentativeAsync(new TentativeQcm
                {
                    UtilisateurId = userId,
                    ModuleId = moduleId,
                    Score = score,
                    Total = total,
                    Reussi = reussi,
                    DateTentative = DateTime.Now
                });

                // 9) Vérifier si TOUS les modules de la formation sont terminés
                bool formationTerminee = false;

                if (await _progressionRepo.HasCompletedAllModulesAsync(userId, module.FormationId))
                {
                    // Passer l'inscription en "Terminé"
                    await _inscriptionRepo.MarkAsCompletedAsync(userId, module.FormationId);

                    // Créer l'attestation automatiquement (idempotent — sans doublon)
                    await _attestationService.CreerOuRecupererAttestationAsync(
                        userId, module.FormationId);

                    formationTerminee = true;
                    message += " 🎓 Formation terminée — votre attestation est disponible !";
                }

                return Ok(new ResultatQcmCompletDto
                {
                    Score = score,
                    Total = total,
                    Reussi = reussi,
                    Pourcentage = pourcentage,
                    Message = message,
                    FormationTerminee = formationTerminee
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la validation du QCM " +
                    $"(module #{moduleId}) : {ex.Message}");
            }
        }

        
        // GET /api/Qcm/{moduleId}/tentatives
        
        //
        // Retourne l'historique des tentatives QCM de l'utilisateur connecté
        // pour un module. Triées de la plus récente à la plus ancienne.
        //
        [HttpGet("{moduleId}/tentatives")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TentativeQcm>>> GetTentatives(int moduleId)
        {
            try
            {
                int userId = GetUserId();

                var tentatives = await _qcmRepo
                    .GetTentativesByUserAndModuleAsync(userId, moduleId);

                return Ok(tentatives);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des tentatives " +
                    $"(module #{moduleId}) : {ex.Message}");
            }
        }

        
        // Helpers privés
        

        //
        // Extrait l'ID utilisateur depuis le claim NameIdentifier du JWT.
        // Lève UnauthorizedAccessException si le claim est absent (token invalide).
        //
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "Token invalide : claim NameIdentifier manquant.");
            return int.Parse(claim.Value);
        }

        //
        // Extrait le RoleId depuis le claim Role du JWT.
        // Retourne 0 si absent.
        //
        private int GetRoleId()
        {
            var claim = User.FindFirst(ClaimTypes.Role);
            return claim != null && int.TryParse(claim.Value, out int roleId) ? roleId : 0;
        }
    }
}
