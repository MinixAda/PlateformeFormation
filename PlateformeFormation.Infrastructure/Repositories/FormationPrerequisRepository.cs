
// Infrastructure/Repositories/FormationPrerequisRepository.cs
//
// Implémentation Dapper du repository des prérequis entre formations.


using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    //
    // Repository Dapper pour la gestion des prérequis entre formations.
    // Exigence TFE : "vérification automatique des prérequis".
    //
    // Chaque ligne de FormationPrerequis signifie :
    // "Pour s'inscrire à FormationId, il faut avoir TERMINÉ FormationRequiseId."
    //
    public class FormationPrerequisRepository : IFormationPrerequisRepository
    {
        private readonly IDbConnection _db;

        public FormationPrerequisRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // GetPrerequisAsync
        
        //
        // Retourne tous les prérequis d'une formation donnée.
        // Utilisé par InscriptionController pour vérifier que tous
        // les prérequis sont satisfaits avant d'autoriser une inscription.
        //
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
                throw new Exception(
                    $"Erreur SQL lors de la récupération des prérequis de la formation #{formationId} : {ex.Message}", ex);
            }
        }

        
        // AddPrerequisAsync
        
        //
        // Ajoute un prérequis à une formation.
        // Vérifie l'existence avant l'insertion pour éviter les doublons.
        // La contrainte CK_NoBoucle en SQL empêche qu'une formation soit
        // son propre prérequis.
        //
        // <returns>true si ajouté, false si le prérequis existait déjà.</returns>
        public async Task<bool> AddPrerequisAsync(int formationId, int formationRequiseId)
        {
            try
            {
                // Vérifier l'existence pour retourner un message explicite
                var checkSql = @"
                    SELECT COUNT(*)
                    FROM FormationPrerequis
                    WHERE FormationId        = @FormationId
                      AND FormationRequiseId = @FormationRequiseId;";

                var exists = await _db.ExecuteScalarAsync<int>(checkSql, new
                {
                    FormationId = formationId,
                    FormationRequiseId = formationRequiseId
                });

                if (exists > 0)
                    return false; // Doublon — le controller renverra BadRequest

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
                throw new Exception(
                    $"Erreur SQL lors de l'ajout du prérequis " +
                    $"(Formation #{formationId} ← Formation #{formationRequiseId}) : {ex.Message}", ex);
            }
        }

        
        // RemovePrerequisAsync
        
        //
        // Supprime un prérequis entre deux formations.
        //
        // <returns>true si supprimé, false si le prérequis n'existait pas.</returns>
        public async Task<bool> RemovePrerequisAsync(int formationId, int formationRequiseId)
        {
            try
            {
                var sql = @"
                    DELETE FROM FormationPrerequis
                    WHERE FormationId        = @FormationId
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
                throw new Exception(
                    $"Erreur SQL lors de la suppression du prérequis " +
                    $"(Formation #{formationId} ← Formation #{formationRequiseId}) : {ex.Message}", ex);
            }
        }

        
        // HasPrerequisAsync
        
        //
        // Vérifie si une formation possède au moins un prérequis.
        // Permet d'optimiser : si false, on saute la boucle de vérification.
        //
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
                throw new Exception(
                    $"Erreur SQL lors de la vérification de l'existence de prérequis " +
                    $"pour la formation #{formationId} : {ex.Message}", ex);
            }
        }
    }
}