using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using System.Data;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Implémentation Dapper du repository Formation.
    // Gère toutes les opérations SQL liées aux formations.
    
    public class FormationRepository : IFormationRepository
    {
        private readonly IDbConnection _db;

        public FormationRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Récupère toutes les formations.
        
        public async Task<IEnumerable<Formation>> GetAllAsync()
        {
            var sql = @"
                SELECT *
                FROM Formation;";

            return await _db.QueryAsync<Formation>(sql);
        }

        
        // Récupère une formation par son ID.
        
        public async Task<Formation?> GetByIdAsync(int id)
        {
            var sql = @"
                SELECT *
                FROM Formation
                WHERE Id = @Id;";

            return await _db.QueryFirstOrDefaultAsync<Formation>(sql, new { Id = id });
        }

        
        // Crée une nouvelle formation.
        
        public async Task CreateAsync(Formation formation)
        {
            var sql = @"
                INSERT INTO Formation 
                (Titre, Description, DateCreation, CreateurId,
                 MediaType, ModeDiffusion, Langue, Niveau,
                 Prerequis, ImageUrl, DureeMinutes)
                VALUES 
                (@Titre, @Description, @DateCreation, @CreateurId,
                 @MediaType, @ModeDiffusion, @Langue, @Niveau,
                 @Prerequis, @ImageUrl, @DureeMinutes);";

            await _db.ExecuteAsync(sql, formation);
        }

        
        // Met à jour une formation existante.
        
        public async Task UpdateAsync(Formation formation)
        {
            var sql = @"
                UPDATE Formation
                SET Titre = @Titre,
                    Description = @Description,
                    MediaType = @MediaType,
                    ModeDiffusion = @ModeDiffusion,
                    Langue = @Langue,
                    Niveau = @Niveau,
                    Prerequis = @Prerequis,
                    ImageUrl = @ImageUrl,
                    DureeMinutes = @DureeMinutes
                WHERE Id = @Id;";

            await _db.ExecuteAsync(sql, formation);
        }

        
        // Supprime une formation.
        
        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM Formation WHERE Id = @Id;";
            await _db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
