// Fichier : PlateformeFormation.Domain/Entities/Attestation.cs

namespace PlateformeFormation.Domain.Entities
{
    //
    // Attestation de suivi générée automatiquement quand un apprenant
    // complète tous les modules d'une formation (Inscription.Statut = "Terminé").
    // Une seule attestation par utilisateur par formation (contrainte UQ_Attestation).
    // NumeroAttestation : identifiant humain unique (ex: "ATT-2026-000042").
    //
    public class Attestation
    {
        //Clé primaire auto-incrémentée.</summary>
        public int Id { get; set; }

        //ID de l'apprenant qui a obtenu l'attestation. FK → Utilisateur.Id.</summary>
        public int UtilisateurId { get; set; }

        //ID de la formation complétée. FK → Formation.Id.</summary>
        public int FormationId { get; set; }

        //Date d'obtention. Assignée par le serveur SQL (DEFAULT GETDATE()).</summary>
        public DateTime DateObtention { get; set; }

        //
        // Numéro lisible unique, format : ATT-YYYY-NNNNNN.
        // Exemple : ATT-2026-000042.
        // Généré par le service lors de la création (pas par SQL).
        // Contrainte SQL : UQ_Attestation_Numero.
        //
        public string NumeroAttestation { get; set; } = string.Empty;
    }
}