using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Repository Dapper gérant la progression des utilisateurs sur les modules.
    // Chaque ligne de ModuleProgression indique qu'un utilisateur a terminé un module.
    
    public class ModuleProgressionRepository : IModuleProgressionRepository
    {
        private readonly IDbConnection _db;

        public ModuleProgressionRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Indique si un module est déjà marqué comme terminé pour un utilisateur.
        
        public async Task<bool> IsModuleCompletedAsync(int userId, int moduleId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM ModuleProgression
                    WHERE UtilisateurId = @UserId
                    AND ModuleId = @ModuleId;";

                return await _db.ExecuteScalarAsync<int>(sql, new { UserId = userId, ModuleId = moduleId }) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la vérification de complétion du module {moduleId} pour l'utilisateur {userId} : {ex.Message}");
            }
        }

        
        // Marque un module comme terminé pour un utilisateur.
        
        public async Task CompleteModuleAsync(int userId, int moduleId)
        {
            try
            {
                var sql = @"
                    INSERT INTO ModuleProgression (UtilisateurId, ModuleId, EstTermine)
                    VALUES (@UserId, @ModuleId, 1);";

                await _db.ExecuteAsync(sql, new { UserId = userId, ModuleId = moduleId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la complétion du module {moduleId} pour l'utilisateur {userId} : {ex.Message}");
            }
        }

        
        // Récupère la progression d'un utilisateur sur une formation donnée.
        
        public async Task<IEnumerable<ModuleProgression>> GetProgressionAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT mp.*
                    FROM ModuleProgression mp
                    INNER JOIN Module m ON m.Id = mp.ModuleId
                    WHERE mp.UtilisateurId = @UserId
                    AND m.FormationId = @FormationId;";

                return await _db.QueryAsync<ModuleProgression>(sql, new { UserId = userId, FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération de la progression (User {userId}, Formation {formationId}) : {ex.Message}");
            }
        }

        
        // Indique si l'utilisateur a terminé tous les modules d'une formation.
        
        public async Task<bool> HasCompletedAllModulesAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        (SELECT COUNT(*) FROM Module WHERE FormationId = @FormationId) AS TotalModules,
                        (SELECT COUNT(*) 
                         FROM ModuleProgression mp
                         INNER JOIN Module m ON m.Id = mp.ModuleId
                         WHERE mp.UtilisateurId = @UserId
                         AND m.FormationId = @FormationId) AS CompletedModules;";

                var result = await _db.QueryFirstAsync<dynamic>(sql, new { UserId = userId, FormationId = formationId });

                int total = result.TotalModules;
                int completed = result.CompletedModules;

                // Si la formation n'a aucun module, on considère qu'elle n'est pas "terminée".
                if (total == 0)
                    return false;

                return completed == total;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la vérification de complétion de la formation {formationId} pour l'utilisateur {userId} : {ex.Message}");
            }
        }
    }
}
