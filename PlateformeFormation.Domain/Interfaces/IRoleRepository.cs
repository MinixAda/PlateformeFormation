
using PlateformeFormation.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlateformeFormation.Domain.Interfaces
{
    // Contrat pour accéder aux rôles
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByNameAsync(string name);
        Task<Role?> GetByIdAsync(int id);
        Task CreateAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(int id);
        Task CreateIfNotExistsAsync(Role role);
    }
}
