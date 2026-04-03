
// PlateformeFormation.Infrastructure/Services/AttestationService.cs
//
// CORRECTION par rapport à la version précédente :
//   - Suppression de "using Microsoft.OpenApi.Models" (erreur CS0246)
//     Ce namespace n'appartient pas à Infrastructure — il est réservé
//     à la couche API (Swashbuckle / Swagger).
//   - GenererDocument() marqué static (avertissement IDE)
//   - Constructeur principal utilisé (avertissement IDE)
//
// Dépendance NuGet à ajouter dans PlateformeFormation.Infrastructure.csproj :
//   <PackageReference Include="QuestPDF" Version="2024.10.3" />


using System;
using System.Globalization;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Services
{
    //
    // Service responsable de :
    //   1. Créer l'attestation en base de données.
    //   2. Générer le PDF de l'attestation (via QuestPDF).
    //   3. Retourner les octets du PDF pour envoi HTTP.
    //
    // Appelé par :
    //   - QcmController.ValiderQcm() (création automatique quand formation terminée)
    //   - AttestationController.DownloadPdf() (téléchargement PDF)
    //
    public class AttestationService(
        IAttestationRepository attestationRepo,
        IUtilisateurRepository utilisateurRepo,
        IFormationRepository formationRepo)
    {
        // Déclaration de licence QuestPDF Community (gratuit pour usage éducatif)
        static AttestationService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private readonly IAttestationRepository _attestationRepo = attestationRepo
            ?? throw new ArgumentNullException(nameof(attestationRepo));
        private readonly IUtilisateurRepository _utilisateurRepo = utilisateurRepo
            ?? throw new ArgumentNullException(nameof(utilisateurRepo));
        private readonly IFormationRepository _formationRepo = formationRepo
            ?? throw new ArgumentNullException(nameof(formationRepo));

        
        // CreerOuRecupererAttestationAsync
        
        //
        // Crée l'attestation en base si elle n'existe pas encore.
        // Idempotent : si elle existe déjà, retourne l'existante sans recréer.
        // Génère le numéro au format ATT-AAAA-NNNNNN (ex: ATT-2026-000042).
        //
        public async Task<Attestation> CreerOuRecupererAttestationAsync(
            int utilisateurId, int formationId)
        {
            try
            {
                // Idempotent — retourner l'existante sans erreur ni doublon
                var existante = await _attestationRepo
                    .GetByUserAndFormationAsync(utilisateurId, formationId);

                if (existante != null)
                    return existante;

                // Générer un numéro unique
                int seq = await _attestationRepo.GetNextSequenceAsync();
                string numero = $"ATT-{DateTime.Now.Year}-{seq:D6}";

                var attestation = new Attestation
                {
                    UtilisateurId = utilisateurId,
                    FormationId = formationId,
                    NumeroAttestation = numero
                    // DateObtention assignée par SQL DEFAULT GETDATE()
                };

                int newId = await _attestationRepo.CreateAsync(attestation);
                attestation.Id = newId;
                attestation.DateObtention = DateTime.Now;

                return attestation;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur création attestation " +
                    $"(Utilisateur #{utilisateurId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }

        
        // GenererPdfAsync
        
        //
        // Génère le PDF de l'attestation et retourne les octets.
        // Charge les données utilisateur, formation et attestation depuis la base.
        //
        public async Task<byte[]> GenererPdfAsync(int utilisateurId, int formationId)
        {
            try
            {
                var utilisateur = await _utilisateurRepo.GetByIdAsync(utilisateurId)
                    ?? throw new Exception($"Utilisateur #{utilisateurId} introuvable.");

                var formation = await _formationRepo.GetByIdAsync(formationId)
                    ?? throw new Exception($"Formation #{formationId} introuvable.");

                var attestation = await _attestationRepo
                    .GetByUserAndFormationAsync(utilisateurId, formationId)
                    ?? throw new Exception(
                        "Attestation introuvable. " +
                        "Vérifiez que la formation est bien terminée.");

                return GenererDocument(utilisateur, formation, attestation);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur génération PDF attestation : {ex.Message}", ex);
            }
        }

        
        // GenererDocument (logique QuestPDF — static car sans état)
        
        //
        // Génère le document PDF via l'API fluent de QuestPDF.
        // Mise en page A4 paysage, design sobre et professionnel.
        // Marqué static car n'accède pas aux champs d'instance.
        //
        private static byte[] GenererDocument(
            Utilisateur utilisateur,
            Formation formation,
            Attestation attestation)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12).FontColor("#212121"));

                    // ---- En-tête ----
                    page.Header().Column(col =>
                    {
                        col.Item().Background("#1565C0").Padding(12).Row(row =>
                        {
                            row.RelativeItem()
                                .Text("FormaPro")
                                .FontSize(24).Bold().FontColor(Colors.White);

                            row.RelativeItem().AlignRight()
                                .Text("Plateforme de Formation en Ligne")
                                .FontSize(10).FontColor("#B3D1F5").Italic();
                        });

                        col.Item().Height(20);
                    });

                    // ---- Corps ----
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().AlignCenter()
                            .Text("ATTESTATION DE SUIVI DE FORMATION")
                            .FontSize(18).Bold().FontColor("#1565C0")
                            .LetterSpacing(2);

                        col.Item().Height(16);

                        col.Item().AlignCenter().Text(text =>
                        {
                            text.Span("La plateforme ").FontSize(13);
                            text.Span("FormaPro").Bold().FontSize(13).FontColor("#1565C0");
                            text.Span(" certifie que").FontSize(13);
                        });

                        col.Item().Height(20);

                        // Nom de l'apprenant
                        col.Item().AlignCenter()
                            .Text($"{utilisateur.Prenom} {utilisateur.Nom.ToUpper()}")
                            .FontSize(30).Bold().FontColor("#0D47A1");

                        col.Item().Height(20);

                        col.Item().AlignCenter()
                            .Text("a suivi et complété avec succès la formation")
                            .FontSize(13);

                        col.Item().Height(12);

                        // Titre de la formation
                        col.Item().AlignCenter()
                            .Text($"« {formation.Titre} »")
                            .FontSize(22).Bold().FontColor("#1565C0");

                        // Niveau et durée
                        if (formation.Niveau != null || formation.DureeMinutes != null)
                        {
                            col.Item().Height(8);
                            col.Item().AlignCenter().Text(text =>
                            {
                                if (formation.Niveau != null)
                                    text.Span($"Niveau : {formation.Niveau}  ")
                                        .FontSize(11).FontColor("#555555");
                                if (formation.DureeMinutes != null)
                                    text.Span($"Durée : {formation.DureeMinutes} min")
                                        .FontSize(11).FontColor("#555555");
                            });
                        }

                        col.Item().Height(20);

                        // Date d'obtention
                        col.Item().AlignCenter().Text(text =>
                        {
                            text.Span("Date d'obtention : ")
                                .FontSize(12).FontColor("#555555");
                            text.Span(attestation.DateObtention
                                .ToString("d MMMM yyyy", new CultureInfo("fr-FR")))
                                .FontSize(12).Bold();
                        });

                        col.Item().Height(30);

                        col.Item().LineHorizontal(1).LineColor("#BBDEFB");
                        col.Item().Height(16);

                        // Numéro d'attestation
                        col.Item().AlignCenter().Text(text =>
                        {
                            text.Span("N° d'attestation : ")
                                .FontSize(10).FontColor("#777777");
                            text.Span(attestation.NumeroAttestation)
                                .FontSize(10).Bold().FontColor("#1565C0");
                        });
                    });

                    // ---- Pied de page ----
                    page.Footer().Background("#F5F5F5").Padding(10).Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Ce document constitue une attestation de suivi et confirme " +
                                  "la complétion de tous les modules de la formation.")
                            .FontSize(8).FontColor("#888888").Italic();

                        row.ConstantItem(120).AlignRight().Text(text =>
                        {
                            text.Span("FormaPro © ").FontSize(8).FontColor("#AAAAAA");
                            text.Span(DateTime.Now.Year.ToString())
                                .FontSize(8).FontColor("#AAAAAA");
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
