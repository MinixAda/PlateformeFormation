
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
    // Implémentation Dapper du repository des signalements de contenu.
    // Réservé aux admins pour les actions de lecture et de traitement.
    //
    public class SignalementRepository : ISignalementRepository
    {
        private readonly IDbConnection _db;

        private const string SelectColumns =
            "Id, SignaleurId, TypeCible, CibleId, Motif, DateSignalement, Statut";

        public SignalementRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IEnumerable<Signalement>> GetEnAttenteAsync()
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Signalement
                    WHERE Statut = N'EnAttente'
                    ORDER BY DateSignalement ASC;";  // Plus ancien en premier → urgence

                return await _db.QueryAsync<Signalement>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des signalements en attente : {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Signalement>> GetAllAsync()
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Signalement
                    ORDER BY DateSignalement DESC;";

                return await _db.QueryAsync<Signalement>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de tous les signalements : {ex.Message}", ex);
            }
        }

        public async Task<Signalement?> GetByIdAsync(int id)
        {
            try
            {
                var sql = $"SELECT {SelectColumns} FROM Signalement WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Signalement>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération du signalement #{id} : {ex.Message}", ex);
            }
        }

        public async Task<int> CreateAsync(Signalement signalement)
        {
            try
            {
                var sql = @"
                    INSERT INTO Signalement
                        (SignaleurId, TypeCible, CibleId, Motif, DateSignalement, Statut)
                    VALUES
                        (@SignaleurId, @TypeCible, @CibleId, @Motif, GETDATE(), N'EnAttente');
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                return await _db.ExecuteScalarAsync<int>(sql, new
                {
                    SignaleurId = signalement.SignaleurId,
                    TypeCible = signalement.TypeCible,
                    CibleId = signalement.CibleId,
                    Motif = signalement.Motif
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création d'un signalement " +
                    $"par l'utilisateur #{signalement.SignaleurId} : {ex.Message}", ex);
            }
        }

        public async Task UpdateStatutAsync(int id, string statut)
        {
            try
            {
                var sql = @"
                    UPDATE Signalement
                    SET Statut = @Statut
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, new { Id = id, Statut = statut });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour du statut du signalement #{id} : {ex.Message}", ex);
            }
        }
    }
}