
// PlateformeFormation.API/Controllers/SignalementController.cs
//
// FICHIER MANQUANT — à créer dans le projet.
//
// Responsabilités :
//   POST  /api/Signalement                        → signaler un contenu (tout user)
//   GET   /api/Signalement                        → liste des signalements (admin)
//   GET   /api/Signalement/en-attente             → signalements en attente (admin)
//   GET   /api/Signalement/{id}                   → détail d'un signalement (admin)
//   PATCH /api/Signalement/{id}/statut            → traiter un signalement (admin)
//
// Exigence TFE :
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
    [Authorize]  // Toutes les actions nécessitent une authentification
    public class SignalementController : ControllerBase
    {
        private readonly ISignalementRepository _signalementRepo;
        private readonly IUtilisateurRepository _utilisateurRepo;

        public SignalementController(
            ISignalementRepository signalementRepo,
            IUtilisateurRepository utilisateurRepo)
        {
            _signalementRepo = signalementRepo
                ?? throw new ArgumentNullException(nameof(signalementRepo));
            _utilisateurRepo = utilisateurRepo
                ?? throw new ArgumentNullException(nameof(utilisateurRepo));
        }

        
        // POST /api/Signalement
        
        //
        // Signale un contenu inapproprié (commentaire ou formation).
        // Disponible pour tout utilisateur connecté.
        //
        // TypeCible acceptés : "Commentaire" | "Formation"
        // Le SignaleurId est extrait du JWT — jamais fourni par le client.
        // Le statut initial est "EnAttente".
        //
        // Un utilisateur ne peut pas se signaler lui-même (contrôle non implémenté
        // ici car nécessiterait de charger la cible — laissé à la modération admin).
        //
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] SignalementCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token invalide : identifiant utilisateur manquant.");

                int signaleurId = int.Parse(userIdClaim.Value);

                // Validation du TypeCible (double vérification en plus du [RegularExpression])
                if (dto.TypeCible != "Commentaire" && dto.TypeCible != "Formation")
                    return BadRequest(
                        "TypeCible doit être 'Commentaire' ou 'Formation'.");

                // Validation du motif
                if (string.IsNullOrWhiteSpace(dto.Motif))
                    return BadRequest("Le motif du signalement est obligatoire.");

                if (dto.Motif.Trim().Length < 10)
                    return BadRequest("Le motif doit contenir au moins 10 caractères.");

                var signalement = new Signalement
                {
                    SignaleurId = signaleurId,
                    TypeCible = dto.TypeCible,
                    CibleId = dto.CibleId,
                    Motif = dto.Motif.Trim(),
                    // DateSignalement et Statut gérés par le repository (GETDATE() et 'EnAttente')
                };

                int newId = await _signalementRepo.CreateAsync(signalement);

                return Ok(
                    $"Signalement enregistré (ID : {newId}). " +
                    "Notre équipe de modération l'examinera dans les meilleurs délais. Merci.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de l'enregistrement du signalement : {ex.Message}");
            }
        }

        
        // GET /api/Signalement
        
        //
        // Retourne tous les signalements (historique complet).
        // Réservé aux administrateurs.
        //
        [HttpGet]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IEnumerable<SignalementReadDto>>> GetAll()
        {
            try
            {
                var signalements = await _signalementRepo.GetAllAsync();
                var result = await EnrichirSignalements(signalements);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des signalements : {ex.Message}");
            }
        }

        
        // GET /api/Signalement/en-attente
        
        //
        // Retourne uniquement les signalements en attente de traitement.
        // Triés du plus ancien au plus récent (urgence de traitement).
        // Réservé aux administrateurs.
        //
        [HttpGet("en-attente")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IEnumerable<SignalementReadDto>>> GetEnAttente()
        {
            try
            {
                var signalements = await _signalementRepo.GetEnAttenteAsync();
                var result = await EnrichirSignalements(signalements);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération des signalements en attente : {ex.Message}");
            }
        }

        
        // GET /api/Signalement/{id}
        
        //
        // Retourne le détail d'un signalement par son ID.
        // Réservé aux administrateurs.
        //
        [HttpGet("{id}")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<SignalementReadDto>> GetById(int id)
        {
            try
            {
                var signalement = await _signalementRepo.GetByIdAsync(id);
                if (signalement == null)
                    return NotFound($"Signalement #{id} introuvable.");

                var signaleur = await _utilisateurRepo.GetByIdAsync(signalement.SignaleurId);

                return Ok(new SignalementReadDto
                {
                    Id = signalement.Id,
                    SignaleurId = signalement.SignaleurId,
                    NomSignaleur = signaleur != null
                        ? $"{signaleur.Prenom} {signaleur.Nom}"
                        : "Utilisateur inconnu",
                    TypeCible = signalement.TypeCible,
                    CibleId = signalement.CibleId,
                    Motif = signalement.Motif,
                    DateSignalement = signalement.DateSignalement,
                    Statut = signalement.Statut
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la récupération du signalement #{id} : {ex.Message}");
            }
        }

        
        // PATCH /api/Signalement/{id}/statut
        
        //
        // Met à jour le statut d'un signalement après examen par l'admin.
        //
        // Statuts valides :
        //   "Traité" → l'admin a pris une action (ex : masqué le commentaire)
        //   "Rejeté" → l'admin a jugé le signalement non fondé
        //
        // Réservé aux administrateurs.
        //
        [HttpPatch("{id}/statut")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult> UpdateStatut(
            int id,
            [FromBody] SignalementUpdateStatutDto dto)
        {
            try
            {
                var signalement = await _signalementRepo.GetByIdAsync(id);
                if (signalement == null)
                    return NotFound($"Signalement #{id} introuvable.");

                // Vérifier le statut actuel
                if (signalement.Statut != "EnAttente")
                    return BadRequest(
                        $"Ce signalement a déjà été traité (statut : {signalement.Statut}). " +
                        "Seuls les signalements 'EnAttente' peuvent être mis à jour.");

                await _signalementRepo.UpdateStatutAsync(id, dto.Statut);

                return Ok($"Signalement #{id} mis à jour : statut → {dto.Statut}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    $"Erreur lors de la mise à jour du signalement #{id} : {ex.Message}");
            }
        }

        
        // Helper privé — enrichissement avec nom signaleur
        

        //
        // Mappe les entités Signalement vers les DTOs en enrichissant
        // le nom du signaleur (jointure C# — évite un SQL JOIN complexe).
        //
        private async Task<IEnumerable<SignalementReadDto>> EnrichirSignalements(
            IEnumerable<Signalement> signalements)
        {
            var result = new List<SignalementReadDto>();

            foreach (var s in signalements)
            {
                var signaleur = await _utilisateurRepo.GetByIdAsync(s.SignaleurId);
                result.Add(new SignalementReadDto
                {
                    Id = s.Id,
                    SignaleurId = s.SignaleurId,
                    NomSignaleur = signaleur != null
                        ? $"{signaleur.Prenom} {signaleur.Nom}"
                        : "Utilisateur inconnu",
                    TypeCible = s.TypeCible,
                    CibleId = s.CibleId,
                    Motif = s.Motif,
                    DateSignalement = s.DateSignalement,
                    Statut = s.Statut
                });
            }

            return result;
        }
    }
}
