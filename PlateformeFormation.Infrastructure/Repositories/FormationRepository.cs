
// Infrastructure/Repositories/FormationRepository.cs
//
// Implémentation Dapper du repository formations et modules.
//
// CORRECTIONS APPLIQUÉES :
//   - EstPublique inclus dans tous les SELECT, INSERT, UPDATE
//   - Colonnes explicites (plus de SELECT *)
//   - Gestion d'exceptions sur chaque méthode avec messages clairs


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
    // Repository Dapper pour la gestion des formations et de leurs modules.
    // Implémente IFormationRepository (lecture rapide des modules).
    // Les opérations CRUD complètes sur les modules passent par ModuleRepository.
    //
    public class FormationRepository : IFormationRepository
    {
        private readonly IDbConnection _db;

        // Colonnes de Formation explicitement listées
        // CORRECTION : EstPublique est maintenant inclus — il était absent,
        // ce qui empêchait Dapper de lire/écrire la visibilité des formations.
        private const string FormationColumns = @"
            Id, Titre, Description, DateCreation, CreateurId,
            MediaType, ModeDiffusion, Langue, Niveau, Prerequis,
            ImageUrl, DureeMinutes, EstPublique";

        // Colonnes de Module explicitement listées
        private const string ModuleColumns =
            "Id, FormationId, Titre, Description, Ordre, DureeMinutes";

        public FormationRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // GetAllAsync
        
        //
        // Retourne toutes les formations (publiques ET privées).
        // Le filtrage de visibilité (EstPublique) est fait dans le controller
        // selon le rôle de l'utilisateur connecté.
        //
        public async Task<IEnumerable<Formation>> GetAllAsync()
        {
            try
            {
                var sql = $"SELECT {FormationColumns} FROM Formation ORDER BY DateCreation DESC;";
                return await _db.QueryAsync<Formation>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de toutes les formations : {ex.Message}", ex);
            }
        }

        
        // GetByIdAsync
        
        //Retourne une formation par son ID. Null si introuvable.</summary>
        public async Task<Formation?> GetByIdAsync(int id)
        {
            try
            {
                var sql = $"SELECT {FormationColumns} FROM Formation WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Formation>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de la formation #{id} : {ex.Message}", ex);
            }
        }

        
        // CreateAsync
        
        //
        // Crée une nouvelle formation en base.
        // DateCreation et CreateurId sont assignés côté serveur (controller).
        // EstPublique est transmis depuis le DTO — permet de créer une formation
        // directement en mode public ou privé.
        //
        public async Task CreateAsync(Formation formation)
        {
            try
            {
                var sql = @"
                    INSERT INTO Formation
                        (Titre, Description, DateCreation, CreateurId,
                         MediaType, ModeDiffusion, Langue, Niveau,
                         Prerequis, ImageUrl, DureeMinutes, EstPublique)
                    VALUES
                        (@Titre, @Description, @DateCreation, @CreateurId,
                         @MediaType, @ModeDiffusion, @Langue, @Niveau,
                         @Prerequis, @ImageUrl, @DureeMinutes, @EstPublique);";

                await _db.ExecuteAsync(sql, formation);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création de la formation '{formation.Titre}' : {ex.Message}", ex);
            }
        }

        
        // UpdateAsync
        
        //
        // Met à jour une formation existante.
        // DateCreation et CreateurId ne sont jamais modifiés.
        // EstPublique peut être modifié pour publier/dépublier une formation.
        //
        public async Task UpdateAsync(Formation formation)
        {
            try
            {
                var sql = @"
                    UPDATE Formation SET
                        Titre         = @Titre,
                        Description   = @Description,
                        MediaType     = @MediaType,
                        ModeDiffusion = @ModeDiffusion,
                        Langue        = @Langue,
                        Niveau        = @Niveau,
                        Prerequis     = @Prerequis,
                        ImageUrl      = @ImageUrl,
                        DureeMinutes  = @DureeMinutes,
                        EstPublique   = @EstPublique
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, formation);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour de la formation #{formation.Id} : {ex.Message}", ex);
            }
        }

        
        // DeleteAsync
        
        //
        // Supprime une formation par son ID.
        // Les modules et inscriptions liés sont supprimés en CASCADE par SQL.
        //
        public async Task DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM Formation WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression de la formation #{id} : {ex.Message}", ex);
            }
        }

        
        // GetModulesByFormationIdAsync
        
        //
        // Retourne tous les modules d'une formation, triés par Ordre.
        // Utilisé par FormationController pour GET /api/formation/{id}/modules.
        //
        public async Task<IEnumerable<Module>> GetModulesByFormationIdAsync(int formationId)
        {
            try
            {
                var sql = $@"
                    SELECT {ModuleColumns}
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

        
        // GetModuleByIdAsync
        
        //
        // Retourne un module par son ID.
        // Utilisé par ModuleProgressionController pour vérifier que le module
        // existe et récupérer son FormationId avant de valider la progression.
        //
        public async Task<Module?> GetModuleByIdAsync(int moduleId)
        {
            try
            {
                var sql = $"SELECT {ModuleColumns} FROM Module WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Module>(sql, new { Id = moduleId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération du module #{moduleId} : {ex.Message}", ex);
            }
        }
    }
}