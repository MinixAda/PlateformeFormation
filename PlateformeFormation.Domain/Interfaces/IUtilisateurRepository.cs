using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    public interface IUtilisateurRepository
    {
        Task<Utilisateur?> GetByEmailAsync(string email);
        Task<Utilisateur?> GetByIdAsync(int id);
        Task<IEnumerable<Utilisateur>> GetAllAsync();

        // Création d'un utilisateur
        Task CreateAsync(Utilisateur user);

        // Mise à jour complète
        Task UpdateAsync(Utilisateur user);

        // Suppression
        Task DeleteAsync(int id);

        // AJOUT : Mise à jour du mot de passe uniquement
        Task UpdatePasswordAsync(Utilisateur utilisateur);
    }
}
