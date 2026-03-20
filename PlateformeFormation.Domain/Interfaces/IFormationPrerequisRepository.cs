using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    
    // Interface définissant les opérations CRUD pour les prérequis de formation.
    
    public interface IFormationPrerequisRepository
    {
        Task<IEnumerable<FormationPrerequis>> GetPrerequisAsync(int formationId);
        Task<bool> AddPrerequisAsync(int formationId, int formationRequiseId);
        Task<bool> RemovePrerequisAsync(int formationId, int formationRequiseId);
        Task<bool> HasPrerequisAsync(int formationId);
    }
}
