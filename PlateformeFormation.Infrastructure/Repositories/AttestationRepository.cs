
// Fichier : PlateformeFormation.Infrastructure/Repositories/AttestationRepository.cs


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
    // Implémentation Dapper du repository des attestations.
    // La génération du numéro (ATT-YYYY-NNNNNN) est faite
    // dans AttestationService avant l'appel à CreateAsync.
    //
    public class AttestationRepository : IAttestationRepository
    {
        private readonly IDbConnection _db;

        private const string SelectColumns =
            "Id, UtilisateurId, FormationId, DateObtention, NumeroAttestation";

        public AttestationRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Attestation?> GetByUserAndFormationAsync(
            int utilisateurId, int formationId)
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Attestation
                    WHERE UtilisateurId = @UtilisateurId
                      AND FormationId   = @FormationId;";

                return await _db.QueryFirstOrDefaultAsync<Attestation>(sql, new
                {
                    UtilisateurId = utilisateurId,
                    FormationId = formationId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de l'attestation " +
                    $"(Utilisateur #{utilisateurId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Attestation>> GetByUserAsync(int utilisateurId)
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Attestation
                    WHERE UtilisateurId = @UtilisateurId
                    ORDER BY DateObtention DESC;";

                return await _db.QueryAsync<Attestation>(sql,
                    new { UtilisateurId = utilisateurId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des attestations " +
                    $"de l'utilisateur #{utilisateurId} : {ex.Message}", ex);
            }
        }

        public async Task<int> CreateAsync(Attestation attestation)
        {
            try
            {
                var sql = @"
                    INSERT INTO Attestation
                        (UtilisateurId, FormationId, DateObtention, NumeroAttestation)
                    VALUES
                        (@UtilisateurId, @FormationId, GETDATE(), @NumeroAttestation);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                return await _db.ExecuteScalarAsync<int>(sql, new
                {
                    UtilisateurId = attestation.UtilisateurId,
                    FormationId = attestation.FormationId,
                    NumeroAttestation = attestation.NumeroAttestation
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création de l'attestation " +
                    $"(Utilisateur #{attestation.UtilisateurId}, Formation #{attestation.FormationId}) : {ex.Message}", ex);
            }
        }

        public async Task<bool> ExisteAsync(int utilisateurId, int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM Attestation
                    WHERE UtilisateurId = @UtilisateurId
                      AND FormationId   = @FormationId;";

                return await _db.ExecuteScalarAsync<int>(sql, new
                {
                    UtilisateurId = utilisateurId,
                    FormationId = formationId
                }) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la vérification d'existence de l'attestation : {ex.Message}", ex);
            }
        }

        public async Task<int> GetNextSequenceAsync()
        {
            try
            {
                // Retourne MAX(Id) + 1, ou 1 si la table est vide
                var sql = @"
                    SELECT ISNULL(MAX(Id), 0) + 1
                    FROM Attestation;";

                return await _db.ExecuteScalarAsync<int>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération du prochain numéro d'attestation : {ex.Message}", ex);
            }
        }
    }
}
