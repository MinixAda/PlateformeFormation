
// ÉTAPE 4 (suite) — Repository Commentaire
// Fichier : PlateformeFormation.Infrastructure/Repositories/CommentaireRepository.cs


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
    // Implémentation Dapper du repository des commentaires.
    // Filtre les commentaires masqués (EstVisible = 0) pour les appels publics.
    //
    public class CommentaireRepository : ICommentaireRepository
    {
        private readonly IDbConnection _db;

        private const string SelectColumns =
            "Id, AuteurId, FormationId, FormateurId, Contenu, DateCommentaire, EstVisible";

        public CommentaireRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        
        // GetByFormationAsync
        
        // <inheritdoc />
        public async Task<IEnumerable<Commentaire>> GetByFormationAsync(int formationId)
        {
            try
            {
                // Seuls les commentaires visibles sont retournés (EstVisible = 1)
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Commentaire
                    WHERE FormationId = @FormationId
                      AND EstVisible  = 1
                    ORDER BY DateCommentaire DESC;";

                return await _db.QueryAsync<Commentaire>(sql,
                    new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des commentaires " +
                    $"de la formation #{formationId} : {ex.Message}", ex);
            }
        }

        
        // GetByFormateurAsync
        
        // <inheritdoc />
        public async Task<IEnumerable<Commentaire>> GetByFormateurAsync(int formateurId)
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM Commentaire
                    WHERE FormateurId = @FormateurId
                      AND EstVisible  = 1
                    ORDER BY DateCommentaire DESC;";

                return await _db.QueryAsync<Commentaire>(sql,
                    new { FormateurId = formateurId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des commentaires " +
                    $"du formateur #{formateurId} : {ex.Message}", ex);
            }
        }

        
        // GetByIdAsync
        
        // <inheritdoc />
        public async Task<Commentaire?> GetByIdAsync(int id)
        {
            try
            {
                var sql = $"SELECT {SelectColumns} FROM Commentaire WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Commentaire>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération du commentaire #{id} : {ex.Message}", ex);
            }
        }

        
        // CreateAsync
        
        // <inheritdoc />
        public async Task<int> CreateAsync(Commentaire commentaire)
        {
            try
            {
                var sql = @"
                    INSERT INTO Commentaire
                        (AuteurId, FormationId, FormateurId, Contenu, DateCommentaire, EstVisible)
                    VALUES
                        (@AuteurId, @FormationId, @FormateurId, @Contenu, GETDATE(), 1);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                return await _db.ExecuteScalarAsync<int>(sql, new
                {
                    AuteurId = commentaire.AuteurId,
                    FormationId = commentaire.FormationId,
                    FormateurId = commentaire.FormateurId,
                    Contenu = commentaire.Contenu
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création d'un commentaire " +
                    $"par l'utilisateur #{commentaire.AuteurId} : {ex.Message}", ex);
            }
        }

        
        // DeleteAsync
        
        // <inheritdoc />
        public async Task DeleteAsync(int id)
        {
            try
            {
                await _db.ExecuteAsync(
                    "DELETE FROM Commentaire WHERE Id = @Id;",
                    new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression du commentaire #{id} : {ex.Message}", ex);
            }
        }

        
        // SetVisibiliteAsync
        
        // <inheritdoc />
        public async Task SetVisibiliteAsync(int id, bool estVisible)
        {
            try
            {
                var sql = @"
                    UPDATE Commentaire
                    SET EstVisible = @EstVisible
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, new { Id = id, EstVisible = estVisible });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors du changement de visibilité du commentaire #{id} : {ex.Message}", ex);
            }
        }
    }
}
