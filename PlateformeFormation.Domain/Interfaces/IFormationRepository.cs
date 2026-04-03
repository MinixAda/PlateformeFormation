
// Domain/Interfaces/IFormationRepository.cs
//
// Contrat pour la gestion des formations et de leurs modules.
// Les modules sont accessibles ici pour les opérations de lecture
// rapide (GetModuleByIdAsync utilisé par ModuleProgressionController).
// Les opérations CRUD complètes sur les modules passent par IModuleRepository.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour l'accès aux données des formations et de leurs modules.
    // Implémenté par FormationRepository dans la couche Infrastructure.
    //
    public interface IFormationRepository
    {
        // ---- Formations ----------------------------------------

        //Retourne toutes les formations (publiques et privées).</summary>
        Task<IEnumerable<Formation>> GetAllAsync();

        //Retourne une formation par son ID. Null si introuvable.</summary>
        Task<Formation?> GetByIdAsync(int id);

        //Crée une nouvelle formation en base.</summary>
        Task CreateAsync(Formation formation);

        //Met à jour une formation existante.</summary>
        Task UpdateAsync(Formation formation);

        //Supprime une formation et ses modules (CASCADE) par son ID.</summary>
        Task DeleteAsync(int id);

        // ---- Modules (lecture rapide) --------------------------

        //
        // Retourne tous les modules d'une formation, triés par Ordre.
        // Utilisé pour la lecture publique depuis FormationController.
        //
        Task<IEnumerable<Module>> GetModulesByFormationIdAsync(int formationId);

        //
        // Retourne un module par son ID.
        // Utilisé par ModuleProgressionController pour vérifier que le module
        // existe et récupérer son FormationId.
        //
        Task<Module?> GetModuleByIdAsync(int moduleId);
    }
}