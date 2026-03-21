using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    // Repository Dapper gérant les inscriptions aux formations.
    // Une inscription lie un utilisateur à une formation, avec un statut.
    
    public class InscriptionRepository : IInscriptionRepository
    {
        private readonly IDbConnection _db;

        public InscriptionRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // Indique si un utilisateur est déjà inscrit à une formation.
        
        public async Task<bool> IsAlreadyInscribedAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM Inscription
                    WHERE UtilisateurId = @UserId
                    AND FormationId = @FormationId;";

                return await _db.ExecuteScalarAsync<int>(sql, new { UserId = userId, FormationId = formationId }) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la vérification d'inscription (User {userId}, Formation {formationId}) : {ex.Message}");
            }
        }

        
        // Indique si un utilisateur a terminé une formation (statut = 'Terminé').
        
        public async Task<bool> HasCompletedFormationAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM Inscription
                    WHERE UtilisateurId = @UserId
                    AND FormationId = @FormationId
                    AND Statut = 'Terminé';";

                return await _db.ExecuteScalarAsync<int>(sql, new { UserId = userId, FormationId = formationId }) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la vérification de complétion (User {userId}, Formation {formationId}) : {ex.Message}");
            }
        }

        
        // Crée une nouvelle inscription.
        
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
                throw new Exception($"Erreur SQL lors de la création de l'inscription (User {inscription.UtilisateurId}, Formation {inscription.FormationId}) : {ex.Message}");
            }
        }

        
        // Récupère toutes les inscriptions d'un utilisateur.
        
        public async Task<IEnumerable<Inscription>> GetByUserAsync(int userId)
        {
            try
            {
                var sql = @"
                    SELECT *
                    FROM Inscription
                    WHERE UtilisateurId = @UserId;";

                return await _db.QueryAsync<Inscription>(sql, new { UserId = userId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la récupération des inscriptions de l'utilisateur {userId} : {ex.Message}");
            }
        }

        
        // Marque une formation comme terminée pour un utilisateur.
        
        public async Task MarkAsCompletedAsync(int userId, int formationId)
        {
            try
            {
                var sql = @"
                    UPDATE Inscription
                    SET Statut = 'Terminé'
                    WHERE UtilisateurId = @UserId
                    AND FormationId = @FormationId;";

                await _db.ExecuteAsync(sql, new { UserId = userId, FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur SQL lors de la mise à jour du statut 'Terminé' (User {userId}, Formation {formationId}) : {ex.Message}");
            }
        }
    }
}
