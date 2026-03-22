using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    
    // Interface définissant les opérations CRUD pour les modules.
    // Utilisée par ModuleRepository (Dapper).
    
    public interface IModuleRepository
    {
        
        // Récupère un module par son identifiant.
        
        Task<Module?> GetByIdAsync(int id);

        
        // Récupère tous les modules d'une formation donnée.
        
        Task<IEnumerable<Module>> GetByFormationIdAsync(int formationId);

        
        // Crée un nouveau module.
        
        Task CreateAsync(Module module);

        
        // Met à jour un module existant.
        
        Task UpdateAsync(Module module);

        
        // Supprime un module par son identifiant.
        
        Task DeleteAsync(int id);
    }
}
