using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Repository Dapper gérant les prérequis entre formations.
    // Chaque ligne de FormationPrerequis indique qu'une formation A
    // nécessite la formation B comme prérequis.
    
    public class FormationPrerequisRepository : IFormationPrerequisRepository
    {
        private readonly IDbConnection _db;

        
        // Le repository reçoit une connexion SQL via l'injection de dépendances.
        
        public FormationPrerequisRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Récupère tous les prérequis d'une formation donnée.
        
        public async Task<IEnumerable<FormationPrerequis>> GetPrerequisAsync(int formationId)
        {
            try
            {
                var sql = @"
                    SELECT FormationId, FormationRequiseId
                    FROM FormationPrerequis
                    WHERE FormationId = @FormationId;";

                return await _db.QueryAsync<FormationPrerequis>(sql, new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                // On remonte une exception claire, qui sera interceptée par le middleware global.
                throw new Exception($"Erreur SQL lors de la récupération des prérequis de la formation {formationId} : {ex.Message}");
            }
        }

        
        // Ajoute un prérequis à une formation.
        // Retourne false si le prérequis existe déjà.
        
        public async Task<bool> AddPrerequisAsync(int formationId, int formationRequiseId)
        {
            try
            {
                // Vérifier si le lien existe déjà pour éviter les doublons.
                var existsSql = @"
                    SELECT COUNT(*)
                    FROM FormationPrerequis
                    WHERE FormationId = @FormationId
                    AND FormationRequiseId = @FormationRequiseId;";

                var exists = await _db.ExecuteScalarAsync<int>(existsSql, new
                {
                    FormationId = formationId,
                    FormationRequiseId = formationRequiseId
                });

                if (exists > 0)
                    return false;

                // Insertion du nouveau prérequis.
                var insertSql = @"
                    INSERT INTO FormationPrerequis (FormationId, FormationRequiseId)
                    VALUES (@FormationId, @FormationRequiseId);";

                await _db.ExecuteAsync(insertSql, new
                {
                    FormationId = formationId,
                    FormationRequiseId = formationRequiseId
                });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de l'ajout du prérequis ({formationId} -> {formationRequiseId}) : {ex.Message}");
            }
        }

        
        // Supprime un prérequis entre deux formations.
        // Retourne true si une ligne a été supprimée.
        
        public async Task<bool> RemovePrerequisAsync(int formationId, int formationRequiseId)
        {
            try
            {
                var sql = @"
                    DELETE FROM FormationPrerequis
                    WHERE FormationId = @FormationId
                    AND FormationRequiseId = @FormationRequiseId;";

                var rows = await _db.ExecuteAsync(sql, new
                {
                    FormationId = formationId,
                    FormationRequiseId = formationRequiseId
                });

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la suppression du prérequis ({formationId} -> {formationRequiseId}) : {ex.Message}");
            }
        }

        
        // Indique si une formation possède au moins un prérequis.
        
        public async Task<bool> HasPrerequisAsync(int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM FormationPrerequis
                    WHERE FormationId = @FormationId;";

                return await _db.ExecuteScalarAsync<int>(sql, new { FormationId = formationId }) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la vérification des prérequis de la formation {formationId} : {ex.Message}");
            }
        }
    }
}
