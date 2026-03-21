using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    
    // Interface pour la gestion de la progression des modules.
    
    public interface IModuleProgressionRepository
    {
        
        // Indique si un module est déjà terminé par un utilisateur.
        
        Task<bool> IsModuleCompletedAsync(int userId, int moduleId);

        
        // Marque un module comme terminé pour un utilisateur.
        
        Task CompleteModuleAsync(int userId, int moduleId);

        
        // Récupère la progression d'un utilisateur sur une formation.
        
        Task<IEnumerable<ModuleProgression>> GetProgressionAsync(int userId, int formationId);

        
        // Indique si l'utilisateur a terminé tous les modules d'une formation.
        
        Task<bool> HasCompletedAllModulesAsync(int userId, int formationId);
    }
}
