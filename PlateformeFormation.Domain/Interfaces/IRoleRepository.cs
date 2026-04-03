
// Domain/Interfaces/IRoleRepository.cs
//
// Contrat pour l'accès aux données des rôles.
// Implémenté par RoleRepository dans la couche Infrastructure.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur les rôles utilisateurs.
    //
    public interface IRoleRepository
    {
        //Retourne tous les rôles disponibles.</summary>
        Task<IEnumerable<Role>> GetAllAsync();

        //Retourne un rôle par son nom exact. Null si introuvable.</summary>
        Task<Role?> GetByNameAsync(string name);

        //Retourne un rôle par son ID. Null si introuvable.</summary>
        Task<Role?> GetByIdAsync(int id);

        //Crée un nouveau rôle en base.</summary>
        Task CreateAsync(Role role);

        //Met à jour le nom d'un rôle existant.</summary>
        Task UpdateAsync(Role role);

        //Supprime un rôle par son ID.</summary>
        Task DeleteAsync(int id);

        //
        // Crée un rôle uniquement s'il n'existe pas déjà en base.
        // Appelé au démarrage de l'application (Program.cs) pour
        // garantir que les rôles Admin, Formateur et Apprenant existent.
        //
        Task CreateIfNotExistsAsync(Role role);
    }
}