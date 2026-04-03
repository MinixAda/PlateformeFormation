// Fichier : PlateformeFormation.Infrastructure/Repositories/SuiviFormateurRepository.cs


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
    // Implémentation Dapper du repository des abonnements apprenant → formateur.
    // La PK composite (ApprenantId, FormateurId) garantit l'unicité côté SQL.
    //
    public class SuiviFormateurRepository : ISuiviFormateurRepository
    {
        private readonly IDbConnection _db;

        public SuiviFormateurRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<bool> SuitDejaAsync(int apprenantId, int formateurId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM SuiviFormateur
                    WHERE ApprenantId = @ApprenantId
                      AND FormateurId = @FormateurId;";

                var count = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    ApprenantId = apprenantId,
                    FormateurId = formateurId
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL vérification suivi " +
                    $"(Apprenant #{apprenantId} → Formateur #{formateurId}) : {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<int>> GetFormateursIdsSuivisAsync(int apprenantId)
        {
            try
            {
                var sql = @"
                    SELECT FormateurId
                    FROM SuiviFormateur
                    WHERE ApprenantId = @ApprenantId
                    ORDER BY DateSuivi DESC;";

                return await _db.QueryAsync<int>(sql, new { ApprenantId = apprenantId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des formateurs suivis " +
                    $"par l'apprenant #{apprenantId} : {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<int>> GetApprenantIdsSuiveursAsync(int formateurId)
        {
            try
            {
                var sql = @"
                    SELECT ApprenantId
                    FROM SuiviFormateur
                    WHERE FormateurId = @FormateurId;";

                return await _db.QueryAsync<int>(sql, new { FormateurId = formateurId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des abonnés " +
                    $"du formateur #{formateurId} : {ex.Message}", ex);
            }
        }

        public async Task SuivreAsync(int apprenantId, int formateurId)
        {
            try
            {
                var sql = @"
                    INSERT INTO SuiviFormateur (ApprenantId, FormateurId, DateSuivi)
                    VALUES (@ApprenantId, @FormateurId, GETDATE());";

                await _db.ExecuteAsync(sql, new
                {
                    ApprenantId = apprenantId,
                    FormateurId = formateurId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de l'abonnement " +
                    $"(Apprenant #{apprenantId} → Formateur #{formateurId}) : {ex.Message}", ex);
            }
        }

        public async Task<bool> NePlusSuivreAsync(int apprenantId, int formateurId)
        {
            try
            {
                var sql = @"
                    DELETE FROM SuiviFormateur
                    WHERE ApprenantId = @ApprenantId
                      AND FormateurId = @FormateurId;";

                var rows = await _db.ExecuteAsync(sql, new
                {
                    ApprenantId = apprenantId,
                    FormateurId = formateurId
                });

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors du désabonnement " +
                    $"(Apprenant #{apprenantId} → Formateur #{formateurId}) : {ex.Message}", ex);
            }
        }
    }
}