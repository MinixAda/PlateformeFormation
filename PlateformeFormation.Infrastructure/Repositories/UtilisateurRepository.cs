using Dapper;
using System.Data;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    public class UtilisateurRepository : IUtilisateurRepository
    {
        private readonly IDbConnection _db;

        public UtilisateurRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<Utilisateur?> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Utilisateur WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<Utilisateur>(sql, new { Email = email });
        }

        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            var sql = "SELECT * FROM Utilisateur WHERE Id = @Id";
            return await _db.QueryFirstOrDefaultAsync<Utilisateur>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Utilisateur>> GetAllAsync()
        {
            var sql = "SELECT * FROM Utilisateur";
            return await _db.QueryAsync<Utilisateur>(sql);
        }

        // Implémentation de CreateAsync (nom aligné sur l'interface)
        public async Task CreateAsync(Utilisateur user)
        {
            var sql = @"
                INSERT INTO Utilisateur (Nom, Prenom, Email, MotDePasseHash, RoleId)
                VALUES (@Nom, @Prenom, @Email, @MotDePasseHash, @RoleId);
            ";

            await _db.ExecuteAsync(sql, user);
        }

        public async Task UpdateAsync(Utilisateur user)
        {
            var sql = @"
                UPDATE Utilisateur
                SET Nom = @Nom,
                    Prenom = @Prenom,
                    Email = @Email,
                    MotDePasseHash = @MotDePasseHash,
                    RoleId = @RoleId
                WHERE Id = @Id;
            ";

            await _db.ExecuteAsync(sql, user);
        }

        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM Utilisateur WHERE Id = @Id";
            await _db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
