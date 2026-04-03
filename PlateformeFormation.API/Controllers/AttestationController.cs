
// PlateformeFormation.API/Controllers/AttestationController.cs
//
// FICHIER MANQUANT — à créer dans le projet.
//
// Responsabilités :
//   GET  /api/Attestation              → liste des attestations de l'utilisateur
//   GET  /api/Attestation/{formationId} → une attestation spécifique
//   GET  /api/Attestation/{formationId}/pdf → téléchargement du PDF
//
// Dépendances :
//   - IAttestationRepository  (lecture en base)
//   - IFormationRepository    (enrichir le DTO avec le titre de la formation)
//   - AttestationService      (génération du PDF QuestPDF)
//
// CORRECTION REQUISE dans Program.cs :
//   builder.Services.AddScoped<IAttestationRepository, AttestationRepository>();
//   builder.Services.AddScoped<AttestationService>();
//   (ces deux lignes manquaient dans le Program.cs original)


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Services;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Toutes les actions nécessitent une authentification
    public class AttestationController : ControllerBase
    {
        private readonly IAttestationRepository _attestationRepo;
        private readonly IFormationRepository _formationRepo;
        private readonly AttestationService _attestationService;

        public AttestationController(
            IAttestationRepository attestationRepo,
            IFormationRepository formationRepo,
            AttestationService attestationService)
        {
            _attestationRepo = attestationRepo
                ?? throw new ArgumentNullException(nameof(attestationRepo));
            _formationRepo = formationRepo
                ?? throw new ArgumentNullException(nameof(formationRepo));
            _attestationService = attestationService
                ?? throw new ArgumentNullException(nameof(attestationService));
        }

        
        // GET /api/Attestation
        
        //
        // Retourne toutes les attestations de l'utilisateur connecté.
        // Enrichit chaque attestation avec le titre et le niveau de la formation.
        // Utilisé par la page "Mes attestations" du frontend.
        //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttestationReadDto>>> GetMesAttestations()
        {
            try
            {
                int userId = GetUserId();

                var attestations = await _attestationRepo.GetByUserAsync(userId);

                // Enrichir les DTOs avec les infos formation pour l'affichage
                var result = new List<AttestationReadDto>();

                foreach (var att in attestations)
                {
                    var formation = await _formationRepo.GetByIdAsync(att.FormationId);

                    result.Add(new AttestationReadDto
                    {
                        Id = att.Id,
                        FormationId = att.FormationId,
                        TitreFormation = formation?.Titre ?? $"Formation #{att.FormationId}",
                        NiveauFormation = formation?.Niveau,
                        DateObtention = att.DateObtention,
                        NumeroAttestation = att.NumeroAttestation
                    });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des attestations : {ex.Message}");
            }
        }

        
        // GET /api/Attestation/{formationId}
        
        //
        // Retourne l'attestation de l'utilisateur connecté pour une formation spécifique.
        // Utile pour vérifier si une attestation existe avant de proposer le PDF.
        // Retourne 404 si l'attestation n'a pas encore été générée
        // (formation non terminée ou attestation inexistante).
        //
        [HttpGet("{formationId}")]
        public async Task<ActionResult<AttestationReadDto>> GetByFormation(int formationId)
        {
            try
            {
                int userId = GetUserId();

                var attestation = await _attestationRepo
                    .GetByUserAndFormationAsync(userId, formationId);

                if (attestation == null)
                    return NotFound(
                        "Attestation introuvable. " +
                        "Vérifiez que vous avez bien terminé toutes les formations requises.");

                var formation = await _formationRepo.GetByIdAsync(formationId);

                return Ok(new AttestationReadDto
                {
                    Id = attestation.Id,
                    FormationId = attestation.FormationId,
                    TitreFormation = formation?.Titre ?? $"Formation #{formationId}",
                    NiveauFormation = formation?.Niveau,
                    DateObtention = attestation.DateObtention,
                    NumeroAttestation = attestation.NumeroAttestation
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération de l'attestation (Formation #{formationId}) : {ex.Message}");
            }
        }

        
        // GET /api/Attestation/{formationId}/pdf
        
        //
        // Génère et retourne le PDF de l'attestation pour une formation donnée.
        // Le PDF est généré via QuestPDF (AttestationService.GenererPdfAsync).
        //
        // Content-Type : application/pdf
        // Content-Disposition : attachment (déclenchement du téléchargement navigateur)
        //
        // Retourne 404 si aucune attestation n'existe pour cette formation.
        //
        [HttpGet("{formationId}/pdf")]
        public async Task<IActionResult> DownloadPdf(int formationId)
        {
            try
            {
                int userId = GetUserId();

                // Vérifier que l'attestation existe avant de générer le PDF
                bool existe = await _attestationRepo.ExisteAsync(userId, formationId);
                if (!existe)
                    return NotFound(
                        "Attestation introuvable. " +
                        "Terminez d'abord tous les modules de la formation.");

                // Générer le PDF via QuestPDF
                byte[] pdfBytes = await _attestationService.GenererPdfAsync(userId, formationId);

                // Construire le nom du fichier
                var attestation = await _attestationRepo
                    .GetByUserAndFormationAsync(userId, formationId);

                string nomFichier = $"attestation_{attestation?.NumeroAttestation ?? "formation"}.pdf";

                // Retourner le PDF en téléchargement direct
                return File(pdfBytes, "application/pdf", nomFichier);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la génération du PDF : {ex.Message}");
            }
        }

        
        // Helper privé
        

        //
        // Extrait l'ID utilisateur depuis le claim NameIdentifier du token JWT.
        // Lève UnauthorizedAccessException si le claim est absent (token invalide).
        //
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "Token invalide : claim NameIdentifier manquant.");
            return int.Parse(claim.Value);
        }
    }
}
