using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Repository Dapper pour la gestion des modules.
    // CRUD complet + récupération par formation.
    
    public class ModuleRepository : IModuleRepository
    {
        private readonly IDbConnection _db;

        public ModuleRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Récupère un module par son identifiant.
        
        public async Task<Module?> GetByIdAsync(int id)
        {
            try
            {
                var sql = "SELECT * FROM Module WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Module>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération du module {id} : {ex.Message}");
            }
        }

        
        // Récupère tous les modules d'une formation, triés par ordre.
        
        public async Task<IEnumerable<Module>> GetByFormationIdAsync(int formationId)
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

        
        // Crée un nouveau module.
        
        public async Task CreateAsync(Module module)
        {
            try
            {
                var sql = @"
                    INSERT INTO Module (FormationId, Titre, Description, Ordre, DureeMinutes)
                    VALUES (@FormationId, @Titre, @Description, @Ordre, @DureeMinutes);";

                await _db.ExecuteAsync(sql, module);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la création du module : {ex.Message}");
            }
        }

        
        // Met à jour un module existant.
        
        public async Task UpdateAsync(Module module)
        {
            try
            {
                var sql = @"
                    UPDATE Module SET
                        Titre = @Titre,
                        Description = @Description,
                        Ordre = @Ordre,
                        DureeMinutes = @DureeMinutes
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, module);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la mise à jour du module {module.Id} : {ex.Message}");
            }
        }

        
        // Supprime un module par son identifiant.
        
        public async Task DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM Module WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la suppression du module {id} : {ex.Message}");
            }
        }
    }
}
