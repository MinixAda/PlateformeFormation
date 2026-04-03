
// PlateformeFormation.API/Controllers/SuiviFormateurController.cs
//
// FICHIER MANQUANT — à créer dans le projet.
//
// Responsabilités :
//   POST   /api/SuiviFormateur                    → s'abonner à un formateur
//   DELETE /api/SuiviFormateur/{formateurId}       → se désabonner
//   GET    /api/SuiviFormateur/mes-formateurs      → liste des formateurs suivis
//   GET    /api/SuiviFormateur/{formateurId}/abonnes → abonnés d'un formateur (admin/formateur)
//
// Exigence TFE :
//   "Suivre des formateurs (Notification de nouveauté)"


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlateformeFormation.API.Dtos;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Tout l'espace requiert une authentification
    public class SuiviFormateurController : ControllerBase
    {
        private readonly ISuiviFormateurRepository _suiviRepo;
        private readonly IUtilisateurRepository _utilisateurRepo;

        public SuiviFormateurController(
            ISuiviFormateurRepository suiviRepo,
            IUtilisateurRepository utilisateurRepo)
        {
            _suiviRepo = suiviRepo
                ?? throw new ArgumentNullException(nameof(suiviRepo));
            _utilisateurRepo = utilisateurRepo
                ?? throw new ArgumentNullException(nameof(utilisateurRepo));
        }

        
        // POST /api/SuiviFormateur
        
        //
        // Abonne l'apprenant connecté à un formateur.
        //
        // Vérifications :
        //   1. Le formateur existe et possède bien le rôle Formateur (RoleId = 2).
        //   2. L'apprenant ne suit pas déjà ce formateur.
        //   3. L'apprenant ne peut pas se suivre lui-même.
        //
        // L'ApprenantId est extrait du JWT — jamais fourni par le client.
        //
        [HttpPost]
        public async Task<ActionResult> Suivre([FromBody] SuivreFormateurDto dto)
        {
            try
            {
                int apprenantId = GetUserId();

                // Un utilisateur ne peut pas se suivre lui-même
                if (apprenantId == dto.FormateurId)
                    return BadRequest("Vous ne pouvez pas vous abonner à votre propre profil.");

                // Vérifier que le formateur existe
                var formateur = await _utilisateurRepo.GetByIdAsync(dto.FormateurId);
                if (formateur == null)
                    return NotFound($"Formateur #{dto.FormateurId} introuvable.");

                // Vérifier que c'est bien un formateur (RoleId = 2) ou admin (RoleId = 1)
                if (formateur.RoleId != 2 && formateur.RoleId != 1)
                    return BadRequest(
                        "Vous pouvez uniquement suivre des formateurs ou des administrateurs.");

                // Vérifier si l'abonnement existe déjà
                if (await _suiviRepo.SuitDejaAsync(apprenantId, dto.FormateurId))
                    return BadRequest(
                        $"Vous suivez déjà {formateur.Prenom} {formateur.Nom}.");

                // Créer l'abonnement
                await _suiviRepo.SuivreAsync(apprenantId, dto.FormateurId);

                return Ok(
                    $"Vous suivez maintenant {formateur.Prenom} {formateur.Nom}. " +
                    "Vous serez notifié de ses nouvelles formations.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de l'abonnement : {ex.Message}");
            }
        }

        
        // DELETE /api/SuiviFormateur/{formateurId}
        
        //
        // Désabonne l'utilisateur connecté d'un formateur.
        // Retourne 404 si l'abonnement n'existait pas.
        //
        [HttpDelete("{formateurId}")]
        public async Task<ActionResult> NePlusSuivre(int formateurId)
        {
            try
            {
                int apprenantId = GetUserId();

                bool supprime = await _suiviRepo.NePlusSuivreAsync(apprenantId, formateurId);

                if (!supprime)
                    return NotFound(
                        $"Vous ne suivez pas le formateur #{formateurId}.");

                return Ok("Désabonnement effectué avec succès.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors du désabonnement : {ex.Message}");
            }
        }

        
        // GET /api/SuiviFormateur/mes-formateurs
        
        //
        // Retourne la liste des formateurs suivis par l'utilisateur connecté.
        // Enrichit chaque entrée avec les informations du formateur (Nom, Prénom, Bio, Portfolio).
        //
        [HttpGet("mes-formateurs")]
        public async Task<ActionResult<IEnumerable<FormateurSuiviReadDto>>> GetMesFormateurs()
        {
            try
            {
                int apprenantId = GetUserId();

                var formateurIds = await _suiviRepo.GetFormateursIdsSuivisAsync(apprenantId);

                var result = new List<FormateurSuiviReadDto>();

                foreach (int formateurId in formateurIds)
                {
                    var formateur = await _utilisateurRepo.GetByIdAsync(formateurId);
                    if (formateur != null)
                    {
                        result.Add(new FormateurSuiviReadDto
                        {
                            Id = formateur.Id,
                            Nom = formateur.Nom,
                            Prenom = formateur.Prenom,
                            Bio = formateur.Bio,
                            LienPortfolio = formateur.LienPortfolio,
                            // DateSuivi non disponible via ce chemin — enrichissement partiel
                            DateSuivi = DateTime.MinValue
                        });
                    }
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
                    $"Erreur lors de la récupération des formateurs suivis : {ex.Message}");
            }
        }

        
        // GET /api/SuiviFormateur/{formateurId}/abonnes
        
        //
        // Retourne le nombre d'abonnés d'un formateur.
        // Réservé aux admins et au formateur lui-même.
        // Permet au formateur de voir son audience.
        //
        [HttpGet("{formateurId}/abonnes")]
        public async Task<ActionResult> GetAbonnes(int formateurId)
        {
            try
            {
                int userId = GetUserId();
                int roleId = GetRoleId();

                // Seul l'admin (1) ou le formateur lui-même peut voir ses abonnés
                if (roleId != 1 && userId != formateurId)
                    return StatusCode(403,
                        "Vous ne pouvez consulter que vos propres abonnés.");

                var abonneIds = await _suiviRepo.GetApprenantIdsSuiveursAsync(formateurId);
                int count = abonneIds.Count();

                return Ok(new
                {
                    FormateurId = formateurId,
                    NombreAbonnes = count,
                    AbonneIds = abonneIds
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des abonnés : {ex.Message}");
            }
        }

        
        // Helpers privés
        

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "Token invalide : claim NameIdentifier manquant.");
            return int.Parse(claim.Value);
        }

        private int GetRoleId()
        {
            var claim = User.FindFirst(ClaimTypes.Role);
            return claim != null && int.TryParse(claim.Value, out int r) ? r : 0;
        }
    }
}
