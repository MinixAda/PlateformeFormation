
// Domain/Interfaces/IFormationPrerequisRepository.cs
//
// Contrat pour la gestion des prérequis entre formations.
// Implémenté par FormationPrerequisRepository dans la couche Infrastructure.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur les prérequis de formations.
    // Exigence TFE : "vérification automatique des prérequis".
    //
    public interface IFormationPrerequisRepository
    {
        //Retourne tous les prérequis d'une formation donnée.</summary>
        Task<IEnumerable<FormationPrerequis>> GetPrerequisAsync(int formationId);

        //
        // Ajoute un prérequis à une formation.
        // Retourne false si le lien existe déjà (évite les doublons).
        //
        Task<bool> AddPrerequisAsync(int formationId, int formationRequiseId);

        //
        // Supprime un prérequis entre deux formations.
        // Retourne false si le lien n'existe pas.
        //
        Task<bool> RemovePrerequisAsync(int formationId, int formationRequiseId);

        //
        // Vérifie si une formation possède au moins un prérequis.
        // Utilisé par InscriptionController avant de créer une inscription.
        //
        Task<bool> HasPrerequisAsync(int formationId);
    }
}