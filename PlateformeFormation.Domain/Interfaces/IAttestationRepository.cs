// Fichier : PlateformeFormation.Domain/Interfaces/IAttestationRepository.cs


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour la gestion des attestations de complétion de formation.
    // Implémenté par AttestationRepository dans Infrastructure.
    //
    public interface IAttestationRepository
    {
        //
        // Retourne l'attestation d'un utilisateur pour une formation.
        // Retourne null si l'attestation n'a pas encore été générée.
        //
        Task<Attestation?> GetByUserAndFormationAsync(int utilisateurId, int formationId);

        //
        // Retourne toutes les attestations d'un utilisateur.
        // Utilisé par la page "Mes attestations".
        //
        Task<IEnumerable<Attestation>> GetByUserAsync(int utilisateurId);

        //
        // Crée une nouvelle attestation. Retourne l'ID généré.
        // Appelé automatiquement par ModuleProgressionController
        // quand tous les modules d'une formation sont terminés.
        //
        Task<int> CreateAsync(Attestation attestation);

        //
        // Vérifie si une attestation existe déjà pour un utilisateur/formation.
        // Empêche les doublons avant d'appeler CreateAsync.
        //
        Task<bool> ExisteAsync(int utilisateurId, int formationId);

        //
        // Retourne le prochain numéro séquentiel pour générer NumeroAttestation.
        // Utilise MAX(Id) + 1 pour garantir l'unicité.
        //
        Task<int> GetNextSequenceAsync();
    }
}