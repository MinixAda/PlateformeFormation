using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Repository Dapper pour la gestion des formations et de leurs modules.
    // Gestion d'exceptions incluse.
    
    public class FormationRepository : IFormationRepository
    {
        private readonly IDbConnection _db;

        public FormationRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Formations
        

        public async Task<IEnumerable<Formation>> GetAllAsync()
        {
            try
            {
                var sql = "SELECT * FROM Formation;";
                return await _db.QueryAsync<Formation>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération des formations : {ex.Message}");
            }
        }

        public async Task<Formation?> GetByIdAsync(int id)
        {
            try
            {
                var sql = "SELECT * FROM Formation WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Formation>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération de la formation {id} : {ex.Message}");
            }
        }

        public async Task CreateAsync(Formation formation)
        {
            try
            {
                var sql = @"
                    INSERT INTO Formation 
                    (Titre, Description, DateCreation, CreateurId, MediaType, ModeDiffusion, Langue, Niveau, Prerequis, ImageUrl, DureeMinutes)
                    VALUES 
                    (@Titre, @Description, @DateCreation, @CreateurId, @MediaType, @ModeDiffusion, @Langue, @Niveau, @Prerequis, @ImageUrl, @DureeMinutes);";

                await _db.ExecuteAsync(sql, formation);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la création de la formation : {ex.Message}");
            }
        }

        public async Task UpdateAsync(Formation formation)
        {
            try
            {
                var sql = @"
                    UPDATE Formation SET
                        Titre = @Titre,
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
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la mise à jour de la formation {formation.Id} : {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM Formation WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la suppression de la formation {id} : {ex.Message}");
            }
        }

        
        // Modules
        

        public async Task<IEnumerable<Module>> GetModulesByFormationIdAsync(int formationId)
        {
            try
            {
                var sql = @"
                    SELECT *
                    FROM Module
                    WHERE FormationId = @FormationId
                    ORDER BY Ordre;";

                return await _db.QueryAsync<Module>(sql, new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération des modules de la formation {formationId} : {ex.Message}");
            }
        }

        public async Task<Module?> GetModuleByIdAsync(int moduleId)
        {
            try
            {
                var sql = "SELECT * FROM Module WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Module>(sql, new { Id = moduleId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération du module {moduleId} : {ex.Message}");
            }
        }
    }
}
