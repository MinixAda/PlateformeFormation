using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    
    // Interface pour la gestion des formations et de leurs modules.
    
    public interface IFormationRepository
    {
        // ------------------------------
        // FORMATIONS
        // ------------------------------

        
        // Récupère toutes les formations.
        
        Task<IEnumerable<Formation>> GetAllAsync();

        
        // Récupère une formation par son identifiant.
        
        Task<Formation?> GetByIdAsync(int id);

        
        // Crée une nouvelle formation.
        
        Task CreateAsync(Formation formation);

        
        // Met à jour une formation existante.
        
        Task UpdateAsync(Formation formation);

        
        // Supprime une formation.
        
        Task DeleteAsync(int id);

        // ------------------------------
        // MODULES
        // ------------------------------

        
        // Récupère tous les modules d'une formation.
        
        Task<IEnumerable<Module>> GetModulesByFormationIdAsync(int formationId);

        
        // Récupère un module par son identifiant.
        
        Task<Module?> GetModuleByIdAsync(int moduleId);
    }
}
