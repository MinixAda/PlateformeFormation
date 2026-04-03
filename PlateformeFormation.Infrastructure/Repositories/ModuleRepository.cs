
// Infrastructure/Repositories/ModuleRepository.cs
//
// Implémentation Dapper du repository des modules.
// CRUD complet : création, lecture, mise à jour, suppression.


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
    // Repository Dapper pour le CRUD complet des modules.
    // Utilisé par FormationController pour gérer les modules d'une formation.
    //
    public class ModuleRepository : IModuleRepository
    {
        private readonly IDbConnection _db;

        // Colonnes explicites — évite les problèmes si la table évolue
        private const string SelectColumns =
            "Id, FormationId, Titre, Description, Ordre, DureeMinutes";

        public ModuleRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // GetByIdAsync
        
        //Retourne un module par son ID. Null si introuvable.</summary>
        public async Task<Module?> GetByIdAsync(int id)
        {
            try
            {
                var sql = $"SELECT {SelectColumns} FROM Module WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Module>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération du module #{id} : {ex.Message}", ex);
            }
        }

        
        // GetByFormationIdAsync
        
        //
        // Retourne tous les modules d'une formation, triés par Ordre.
        // L'ordre est important pour l'affichage séquentiel à l'apprenant.
        //
        public async Task<IEnumerable<Module>> GetByFormationIdAsync(int formationId)
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Module
                    WHERE FormationId = @FormationId
                    ORDER BY Ordre;";

                return await _db.QueryAsync<Module>(sql, new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des modules de la formation #{formationId} : {ex.Message}", ex);
            }
        }

        
        // CreateAsync
        
        //
        // Crée un nouveau module et l'associe à une formation.
        // L'Ordre détermine la position du module dans la séquence pédagogique.
        //
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
                throw new Exception(
                    $"Erreur SQL lors de la création du module '{module.Titre}' (Formation #{module.FormationId}) : {ex.Message}", ex);
            }
        }

        
        // UpdateAsync
        
        //
        // Met à jour un module existant.
        // FormationId ne peut pas être modifié — un module reste lié à sa formation.
        //
        public async Task UpdateAsync(Module module)
        {
            try
            {
                var sql = @"
                    UPDATE Module SET
                        Titre        = @Titre,
                        Description  = @Description,
                        Ordre        = @Ordre,
                        DureeMinutes = @DureeMinutes
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, module);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour du module #{module.Id} : {ex.Message}", ex);
            }
        }

        
        // DeleteAsync
        
        //
        // Supprime un module par son ID.
        // Les progressions liées sont supprimées en CASCADE par SQL.
        //
        public async Task DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM Module WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression du module #{id} : {ex.Message}", ex);
            }
        }
    }
}