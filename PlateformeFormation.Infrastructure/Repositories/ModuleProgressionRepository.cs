
// Infrastructure/Repositories/ModuleProgressionRepository.cs
//
// Implémentation Dapper du repository de progression des modules.
//
// CORRECTION APPLIQUÉE :
//   CompleteModuleAsync insère maintenant DateCompletion = GETDATE()
//   (la date était à 1900-01-01 sans cette correction).


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
    // Repository Dapper pour la progression des apprenants sur les modules.
    // Chaque ligne représente un module terminé par un utilisateur à une date donnée.
    //
    public class ModuleProgressionRepository : IModuleProgressionRepository
    {
        private readonly IDbConnection _db;

        public ModuleProgressionRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // IsModuleCompletedAsync
        
        //
        // Vérifie si un module est déjà enregistré comme terminé pour un utilisateur.
        // Empêche les doublons dans la table ModuleProgression
        // (contrainte UQ_ModuleProgression en SQL en dernier recours).
        //
        public async Task<bool> IsModuleCompletedAsync(int userId, int moduleId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM ModuleProgression
                    WHERE UtilisateurId = @UserId
                      AND ModuleId     = @ModuleId;";

                var count = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    UserId = userId,
                    ModuleId = moduleId
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la vérification de complétion du module #{moduleId} " +
                    $"pour l'utilisateur #{userId} : {ex.Message}", ex);
            }
        }

        
        // CompleteModuleAsync — CORRECTION APPLIQUÉE
        
        //
        // Enregistre un module comme terminé pour un utilisateur.
        //
        // CORRECTION : DateCompletion est maintenant inséré avec GETDATE()
        // (heure serveur SQL). Sans cette correction, la colonne restait
        // à sa valeur par défaut SQL (GETDATE() défini en DEFAULT dans le DDL),
        // mais le C# sans ce paramètre envoyait DateTime.MinValue (1900-01-01)
        // qui écrasait le DEFAULT. On force maintenant explicitement GETDATE().
        //
        public async Task CompleteModuleAsync(int userId, int moduleId)
        {
            try
            {
                var sql = @"
                    INSERT INTO ModuleProgression (UtilisateurId, ModuleId, EstTermine, DateCompletion)
                    VALUES (@UserId, @ModuleId, 1, GETDATE());";

                await _db.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    ModuleId = moduleId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de l'enregistrement de la complétion du module #{moduleId} " +
                    $"pour l'utilisateur #{userId} : {ex.Message}", ex);
            }
        }

        
        // GetProgressionAsync
        
        //
        // Retourne la liste des modules terminés par un utilisateur pour une formation.
        // Utilisé par ProgressionPage (frontend) et AttestationPage.
        // La jointure avec Module permet de filtrer par FormationId.
        //
        public async Task<IEnumerable<ModuleProgression>> GetProgressionAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT mp.Id, mp.UtilisateurId, mp.ModuleId, mp.EstTermine, mp.DateCompletion
                    FROM ModuleProgression mp
                    INNER JOIN Module m ON m.Id = mp.ModuleId
                    WHERE mp.UtilisateurId = @UserId
                      AND m.FormationId   = @FormationId
                    ORDER BY mp.DateCompletion;";

                return await _db.QueryAsync<ModuleProgression>(sql, new
                {
                    UserId = userId,
                    FormationId = formationId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de la progression " +
                    $"(Utilisateur #{userId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }

        
        // HasCompletedAllModulesAsync
        
        //
        // Vérifie si un utilisateur a terminé TOUS les modules d'une formation.
        // Si true → ModuleProgressionController déclenche MarkAsCompletedAsync.
        //
        // Retourne false si la formation n'a aucun module
        // (impossible de "terminer" une formation vide).
        //
        // La requête calcule en une seule passe :
        //   - TotalModules    : nombre total de modules dans la formation
        //   - CompletedModules : nombre de modules terminés par l'utilisateur
        //
        public async Task<bool> HasCompletedAllModulesAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT
                        (SELECT COUNT(*)
                         FROM Module
                         WHERE FormationId = @FormationId) AS TotalModules,

                        (SELECT COUNT(*)
                         FROM ModuleProgression mp
                         INNER JOIN Module m ON m.Id = mp.ModuleId
                         WHERE mp.UtilisateurId = @UserId
                           AND m.FormationId   = @FormationId) AS CompletedModules;";

                var result = await _db.QueryFirstAsync<dynamic>(sql, new
                {
                    UserId = userId,
                    FormationId = formationId
                });

                int total = (int)result.TotalModules;
                int completed = (int)result.CompletedModules;

                // Une formation sans modules ne peut pas être "terminée"
                if (total == 0) return false;

                return completed == total;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la vérification de complétion complète " +
                    $"(Utilisateur #{userId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }
    }
}