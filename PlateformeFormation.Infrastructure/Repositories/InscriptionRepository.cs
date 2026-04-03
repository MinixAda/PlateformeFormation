
// Infrastructure/Repositories/InscriptionRepository.cs
//
// Implémentation Dapper du repository des inscriptions.


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
    // Repository Dapper pour la gestion des inscriptions aux formations.
    // Une inscription lie un utilisateur à une formation avec un statut.
    // Statuts possibles : "EnCours" | "Terminé"
    //
    public class InscriptionRepository : IInscriptionRepository
    {
        private readonly IDbConnection _db;

        public InscriptionRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // IsAlreadyInscribedAsync
        
        //
        // Vérifie si un utilisateur est déjà inscrit à une formation.
        // Appelé avant CreateAsync pour éviter les doublons
        // (contrainte UQ_Inscription est aussi en SQL en dernier recours).
        //
        public async Task<bool> IsAlreadyInscribedAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM Inscription
                    WHERE UtilisateurId = @UserId
                      AND FormationId  = @FormationId;";

                var count = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    UserId = userId,
                    FormationId = formationId
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la vérification d'inscription " +
                    $"(Utilisateur #{userId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }

        
        // HasCompletedFormationAsync
        
        //
        // Vérifie si un utilisateur a terminé une formation (statut = "Terminé").
        // Utilisé par InscriptionController pour valider les prérequis :
        // l'utilisateur doit avoir TERMINÉ la formation prérequise
        // avant de s'inscrire à la formation cible.
        //
        public async Task<bool> HasCompletedFormationAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM Inscription
                    WHERE UtilisateurId = @UserId
                      AND FormationId  = @FormationId
                      AND Statut       = N'Terminé';";

                // Note : N'Terminé' pour garantir la gestion de l'accent
                // avec toutes les collations SQL Server.

                var count = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    UserId = userId,
                    FormationId = formationId
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la vérification de complétion " +
                    $"(Utilisateur #{userId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }

        
        // CreateAsync
        
        //
        // Crée une nouvelle inscription avec le statut "EnCours".
        // DateInscription et Statut sont assignés côté serveur (controller).
        //
        public async Task CreateAsync(Inscription inscription)
        {
            try
            {
                var sql = @"
                    INSERT INTO Inscription (UtilisateurId, FormationId, DateInscription, Statut)
                    VALUES (@UtilisateurId, @FormationId, @DateInscription, @Statut);";

                await _db.ExecuteAsync(sql, inscription);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création de l'inscription " +
                    $"(Utilisateur #{inscription.UtilisateurId}, Formation #{inscription.FormationId}) : {ex.Message}", ex);
            }
        }

        
        // GetByUserAsync
        
        //
        // Retourne toutes les inscriptions d'un utilisateur.
        // Utilisé par GET /api/Inscription/mes-inscriptions.
        // Le frontend charge ensuite les détails de chaque formation par ID.
        //
        public async Task<IEnumerable<Inscription>> GetByUserAsync(int userId)
        {
            try
            {
                var sql = @"
                    SELECT Id, UtilisateurId, FormationId, DateInscription, Statut
                    FROM Inscription
                    WHERE UtilisateurId = @UserId
                    ORDER BY DateInscription DESC;";

                return await _db.QueryAsync<Inscription>(sql, new { UserId = userId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des inscriptions de l'utilisateur #{userId} : {ex.Message}", ex);
            }
        }

        
        // MarkAsCompletedAsync
        
        //
        // Passe le statut d'une inscription à "Terminé".
        // Appelé automatiquement par ModuleProgressionController
        // quand HasCompletedAllModulesAsync() retourne true.
        // Déclenche l'éligibilité à l'attestation de suivi.
        //
        public async Task MarkAsCompletedAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    UPDATE Inscription
                    SET Statut = N'Terminé'
                    WHERE UtilisateurId = @UserId
                      AND FormationId  = @FormationId;";

                await _db.ExecuteAsync(sql, new
                {
                    UserId = userId,
                    FormationId = formationId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors du passage en statut 'Terminé' " +
                    $"(Utilisateur #{userId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }
    }
}