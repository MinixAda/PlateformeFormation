using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    
    // Interface pour la gestion des inscriptions aux formations.
    
    public interface IInscriptionRepository
    {
        
        // Indique si un utilisateur est déjà inscrit à une formation.
        
        Task<bool> IsAlreadyInscribedAsync(int userId, int formationId);

        
        // Indique si un utilisateur a terminé une formation.
        
        Task<bool> HasCompletedFormationAsync(int userId, int formationId);

        
        // Crée une nouvelle inscription.
        
        Task CreateAsync(Inscription inscription);

        
        // Récupère toutes les inscriptions d'un utilisateur.
        
        Task<IEnumerable<Inscription>> GetByUserAsync(int userId);

        
        // Marque une formation comme terminée pour un utilisateur.
        
        Task MarkAsCompletedAsync(int userId, int formationId);
    }
}
