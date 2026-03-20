using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Repository Dapper gérant les prérequis entre formations.
    // Contient une gestion explicite des doublons et des erreurs.
    
    public class FormationPrerequisRepository : IFormationPrerequisRepository
    {
        private readonly IDbConnection _db;

        public FormationPrerequisRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Récupère tous les prérequis d'une formation.
        
        public async Task<IEnumerable<FormationPrerequis>> GetPrerequisAsync(int formationId)
        {
            var sql = @"
                SELECT FormationId, FormationRequiseId
                FROM FormationPrerequis
                WHERE FormationId = @FormationId;";

            return await _db.QueryAsync<FormationPrerequis>(sql, new { FormationId = formationId });
        }

        
        // Ajoute un prérequis à une formation.
        // Retourne false si le prérequis existe déjà.
        
        public async Task<bool> AddPrerequisAsync(int formationId, int formationRequiseId)
        {
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
            {
                // Le prérequis existe déjà, on ne réinsère pas
                return false;
            }

            var insertSql = @"
                INSERT INTO FormationPrerequis (FormationId, FormationRequiseId)
                VALUES (@FormationId, @FormationRequiseId);";

            try
            {
                await _db.ExecuteAsync(insertSql, new
                {
                    FormationId = formationId,
                    FormationRequiseId = formationRequiseId
                });
            }
            catch (Exception ex)
            {
                // On remonte une erreur explicite au niveau supérieur
                throw new Exception(
                    $"Erreur lors de l'ajout du prérequis (FormationId={formationId}, FormationRequiseId={formationRequiseId}) : {ex.Message}");
            }

            return true;
        }

        
        // Supprime un prérequis.
        // Retourne false si aucune ligne n'a été supprimée (prérequis inexistant).
        
        public async Task<bool> RemovePrerequisAsync(int formationId, int formationRequiseId)
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

        
        // Vérifie si une formation possède au moins un prérequis.
        
        public async Task<bool> HasPrerequisAsync(int formationId)
        {
            var sql = @"
                SELECT COUNT(*)
                FROM FormationPrerequis
                WHERE FormationId = @FormationId;";

            return await _db.ExecuteScalarAsync<int>(sql, new { FormationId = formationId }) > 0;
        }
    }
}
