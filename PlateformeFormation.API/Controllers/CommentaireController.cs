
// PlateformeFormation.API/Controllers/CommentaireController.cs
//
// FICHIER MANQUANT — à créer dans le projet.
//
// Responsabilités :
//   GET    /api/Commentaire/formation/{formationId}  → commentaires d'une formation
//   GET    /api/Commentaire/formateur/{formateurId}  → commentaires sur un formateur
//   POST   /api/Commentaire                          → poster un commentaire
//   DELETE /api/Commentaire/{id}                     → supprimer (auteur ou admin)
//   PATCH  /api/Commentaire/{id}/visibilite          → masquer/rétablir (admin seul)
//
// Exigences TFE respectées :
//   "Poster des commentaires sur : formation / parcours / formateur"
//   "Signalement de comportement (ciblant commentaire ou une formation)"


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
    public class CommentaireController : ControllerBase
    {
        private readonly ICommentaireRepository _commentaireRepo;
        private readonly IUtilisateurRepository _utilisateurRepo;
        private readonly IFormationRepository _formationRepo;

        public CommentaireController(
            ICommentaireRepository commentaireRepo,
            IUtilisateurRepository utilisateurRepo,
            IFormationRepository formationRepo)
        {
            _commentaireRepo = commentaireRepo
                ?? throw new ArgumentNullException(nameof(commentaireRepo));
            _utilisateurRepo = utilisateurRepo
                ?? throw new ArgumentNullException(nameof(utilisateurRepo));
            _formationRepo = formationRepo
                ?? throw new ArgumentNullException(nameof(formationRepo));
        }

        
        // GET /api/Commentaire/formation/{formationId}
        
        //
        // Retourne les commentaires visibles d'une formation.
        // Public — affiché sur la page de détail d'une formation.
        // Inclut le nom de l'auteur pour l'affichage (joint depuis Utilisateur).
        // Les commentaires masqués par un admin (EstVisible = false) sont exclus.
        //
        [HttpGet("formation/{formationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentaireReadDto>>> GetByFormation(int formationId)
        {
            try
            {
                // Vérifier que la formation existe
                var formation = await _formationRepo.GetByIdAsync(formationId);
                if (formation == null)
                    return NotFound($"Formation #{formationId} introuvable.");

                var commentaires = await _commentaireRepo.GetByFormationAsync(formationId);

                // Enrichir avec le nom de l'auteur
                var result = new List<CommentaireReadDto>();
                foreach (var c in commentaires)
                {
                    var auteur = await _utilisateurRepo.GetByIdAsync(c.AuteurId);
                    result.Add(new CommentaireReadDto
                    {
                        Id = c.Id,
                        AuteurId = c.AuteurId,
                        NomAuteur = auteur != null
                            ? $"{auteur.Prenom} {auteur.Nom}"
                            : "Utilisateur inconnu",
                        FormationId = c.FormationId,
                        FormateurId = c.FormateurId,
                        Contenu = c.Contenu,
                        DateCommentaire = c.DateCommentaire,
                        EstVisible = c.EstVisible
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des commentaires " +
                    $"(Formation #{formationId}) : {ex.Message}");
            }
        }

        
        // GET /api/Commentaire/formateur/{formateurId}
        
        //
        // Retourne les commentaires visibles laissés sur un formateur.
        // Public — affiché sur la page de profil du formateur.
        // Exigence TFE : "Poster des commentaires sur : formateur".
        //
        [HttpGet("formateur/{formateurId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentaireReadDto>>> GetByFormateur(int formateurId)
        {
            try
            {
                var commentaires = await _commentaireRepo.GetByFormateurAsync(formateurId);

                var result = new List<CommentaireReadDto>();
                foreach (var c in commentaires)
                {
                    var auteur = await _utilisateurRepo.GetByIdAsync(c.AuteurId);
                    result.Add(new CommentaireReadDto
                    {
                        Id = c.Id,
                        AuteurId = c.AuteurId,
                        NomAuteur = auteur != null
                            ? $"{auteur.Prenom} {auteur.Nom}"
                            : "Utilisateur inconnu",
                        FormationId = c.FormationId,
                        FormateurId = c.FormateurId,
                        Contenu = c.Contenu,
                        DateCommentaire = c.DateCommentaire,
                        EstVisible = c.EstVisible
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des commentaires " +
                    $"du formateur #{formateurId} : {ex.Message}");
            }
        }

        
        // POST /api/Commentaire
        
        //
        // Poste un nouveau commentaire sur une formation ou un formateur.
        // L'AuteurId est extrait du JWT — jamais fourni par le client.
        //
        // Règles de validation :
        //   - FormationId OU FormateurId doit être renseigné (au moins un).
        //   - Le contenu doit contenir entre 10 et 2000 caractères.
        //   - L'auteur ne peut pas commenter une formation qui n'existe pas.
        //
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CommentaireCreateDto dto)
        {
            try
            {
                // Validation : au moins une cible doit être renseignée
                if (dto.FormationId == null && dto.FormateurId == null)
                    return BadRequest(
                        "Vous devez cibler une formation (FormationId) " +
                        "ou un formateur (FormateurId).");

                // Validation du contenu
                if (string.IsNullOrWhiteSpace(dto.Contenu))
                    return BadRequest("Le contenu du commentaire est obligatoire.");

                if (dto.Contenu.Trim().Length < 10)
                    return BadRequest("Le commentaire doit contenir au moins 10 caractères.");

                if (dto.Contenu.Trim().Length > 2000)
                    return BadRequest("Le commentaire ne peut pas dépasser 2000 caractères.");

                // Extraire l'ID auteur depuis le JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int auteurId = int.Parse(userIdClaim.Value);

                // Vérifier que la formation existe (si ciblée)
                if (dto.FormationId.HasValue)
                {
                    var formation = await _formationRepo.GetByIdAsync(dto.FormationId.Value);
                    if (formation == null)
                        return BadRequest($"La formation #{dto.FormationId} n'existe pas.");
                }

                // Vérifier que le formateur existe (si ciblé)
                if (dto.FormateurId.HasValue)
                {
                    var formateur = await _utilisateurRepo.GetByIdAsync(dto.FormateurId.Value);
                    if (formateur == null)
                        return BadRequest($"Le formateur #{dto.FormateurId} n'existe pas.");
                }

                var commentaire = new Commentaire
                {
                    AuteurId = auteurId,
                    FormationId = dto.FormationId,
                    FormateurId = dto.FormateurId,
                    Contenu = dto.Contenu.Trim(),
                    // DateCommentaire et EstVisible gérés par le repository (GETDATE() et DEFAULT 1)
                };

                int newId = await _commentaireRepo.CreateAsync(commentaire);

                return Ok($"Commentaire publié avec succès (ID : {newId}).");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la publication du commentaire : {ex.Message}");
            }
        }

        
        // DELETE /api/Commentaire/{id}
        
        //
        // Supprime un commentaire.
        // Règle : seul l'auteur du commentaire ou un administrateur peut le supprimer.
        // Un utilisateur ne peut pas supprimer le commentaire d'un autre utilisateur.
        //
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var commentaire = await _commentaireRepo.GetByIdAsync(id);
                if (commentaire == null)
                    return NotFound($"Commentaire #{id} introuvable.");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int userId = int.Parse(userIdClaim.Value);
                var roleClaim = User.FindFirst(ClaimTypes.Role);
                int roleId = roleClaim != null && int.TryParse(roleClaim.Value, out int r) ? r : 0;

                // Vérification : auteur ou admin uniquement
                bool estAdmin = roleId == 1;
                bool estAuteur = commentaire.AuteurId == userId;

                if (!estAdmin && !estAuteur)
                    return StatusCode(403,
                        "Vous ne pouvez supprimer que vos propres commentaires.");

                await _commentaireRepo.DeleteAsync(id);
                return Ok($"Commentaire #{id} supprimé avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la suppression du commentaire #{id} : {ex.Message}");
            }
        }

        
        // PATCH /api/Commentaire/{id}/visibilite
        
        //
        // Modifie la visibilité d'un commentaire (masquer ou rétablir).
        // Réservé aux administrateurs.
        // Utilisé après le traitement d'un signalement :
        //   - Masquer (false) : le commentaire n'est plus visible publiquement.
        //   - Rétablir (true) : le commentaire redevient visible.
        // Le commentaire reste en base pour garder l'historique.
        //
        [HttpPatch("{id}/visibilite")]
        [Authorize(Roles = "1")]  // Admin uniquement
        public async Task<ActionResult> SetVisibilite(int id, [FromQuery] bool estVisible)
        {
            try
            {
                var commentaire = await _commentaireRepo.GetByIdAsync(id);
                if (commentaire == null)
                    return NotFound($"Commentaire #{id} introuvable.");

                await _commentaireRepo.SetVisibiliteAsync(id, estVisible);

                string action = estVisible ? "rendu visible" : "masqué";
                return Ok($"Commentaire #{id} {action} avec succès.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la modification de la visibilité " +
                    $"du commentaire #{id} : {ex.Message}");
            }
        }
    }
}
