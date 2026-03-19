using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    
    // Interface définissant les opérations CRUD pour les formations.
    
    public interface IFormationRepository
    {
        Task<IEnumerable<Formation>> GetAllAsync();
        Task<Formation?> GetByIdAsync(int id);
        Task CreateAsync(Formation formation);
        Task UpdateAsync(Formation formation);
        Task DeleteAsync(int id);
    }
}
