
// Domain/Interfaces/IModuleRepository.cs
//
// Contrat pour le CRUD complet sur les modules.
// Implémenté par ModuleRepository dans la couche Infrastructure.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations CRUD sur les modules d'une formation.
    // Utilisé par FormationController pour créer, modifier et supprimer les modules.
    //
    public interface IModuleRepository
    {
        //Retourne un module par son ID. Null si introuvable.</summary>
        Task<Module?> GetByIdAsync(int id);

        //Retourne tous les modules d'une formation, triés par Ordre.</summary>
        Task<IEnumerable<Module>> GetByFormationIdAsync(int formationId);

        //Crée un nouveau module en base.</summary>
        Task CreateAsync(Module module);

        //Met à jour un module existant.</summary>
        Task UpdateAsync(Module module);

        //Supprime un module par son ID.</summary>
        Task DeleteAsync(int id);
    }
}