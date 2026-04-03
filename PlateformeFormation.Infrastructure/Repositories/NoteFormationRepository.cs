
// ÉTAPE 4 — Repository Infrastructure
// Fichier : PlateformeFormation.Infrastructure/Repositories/NoteFormationRepository.cs


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
    // Implémentation Dapper du repository des notes de formation.
    // Gère le UPSERT (créer ou mettre à jour) en une seule requête SQL
    // via MERGE pour éviter les race conditions.
    //
    public class NoteFormationRepository : INoteFormationRepository
    {
        private readonly IDbConnection _db;

        //
        // Colonnes sélectionnées explicitement pour éviter les surprises
        // si la table évolue.
        //
        private const string SelectColumns =
            "Id, UtilisateurId, FormationId, Note, DateNote";

        public NoteFormationRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        
        // GetNoteUtilisateurAsync
        
        // <inheritdoc />
        public async Task<NoteFormation?> GetNoteUtilisateurAsync(
            int utilisateurId, int formationId)
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM NoteFormation
                    WHERE UtilisateurId = @UtilisateurId
                      AND FormationId   = @FormationId;";

                return await _db.QueryFirstOrDefaultAsync<NoteFormation>(sql, new
                {
                    UtilisateurId = utilisateurId,
                    FormationId = formationId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de la note " +
                    $"(Utilisateur #{utilisateurId}, Formation #{formationId}) : {ex.Message}", ex);
            }
        }

        
        // GetNotesByFormationAsync
        
        // <inheritdoc />
        public async Task<IEnumerable<NoteFormation>> GetNotesByFormationAsync(int formationId)
        {
            try
            {
                var sql = $@"
                    SELECT {SelectColumns}
                    FROM NoteFormation
                    WHERE FormationId = @FormationId
                    ORDER BY DateNote DESC;";

                return await _db.QueryAsync<NoteFormation>(sql,
                    new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des notes " +
                    $"de la formation #{formationId} : {ex.Message}", ex);
            }
        }

        
        // GetMoyenneAsync
        
        // <inheritdoc />
        public async Task<decimal?> GetMoyenneAsync(int formationId)
        {
            try
            {
                // AVG retourne NULL si aucune ligne → correspondance avec decimal?
                var sql = @"
                    SELECT AVG(CAST(Note AS DECIMAL(10,2)))
                    FROM NoteFormation
                    WHERE FormationId = @FormationId;";

                return await _db.ExecuteScalarAsync<decimal?>(sql,
                    new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors du calcul de la moyenne " +
                    $"de la formation #{formationId} : {ex.Message}", ex);
            }
        }

        
        // CountNotesAsync
        
        // <inheritdoc />
        public async Task<int> CountNotesAsync(int formationId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM NoteFormation
                    WHERE FormationId = @FormationId;";

                return await _db.ExecuteScalarAsync<int>(sql,
                    new { FormationId = formationId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors du comptage des notes " +
                    $"de la formation #{formationId} : {ex.Message}", ex);
            }
        }

        
        // UpsertNoteAsync
        
        // <inheritdoc />
        // <remarks>
        // Utilise MERGE pour faire un UPSERT atomique :
        // - Si la note existe déjà → UPDATE.
        // - Si elle n'existe pas → INSERT.
        // Évite la race condition d'un SELECT suivi d'un INSERT ou UPDATE séparés.
        // </remarks>
        public async Task UpsertNoteAsync(NoteFormation note)
        {
            try
            {
                var sql = @"
                    MERGE NoteFormation AS target
                    USING (
                        SELECT @UtilisateurId AS UtilisateurId,
                               @FormationId   AS FormationId,
                               @Note          AS Note
                    ) AS source
                        ON target.UtilisateurId = source.UtilisateurId
                       AND target.FormationId   = source.FormationId
                    WHEN MATCHED THEN
                        UPDATE SET Note     = source.Note,
                                   DateNote = GETDATE()
                    WHEN NOT MATCHED THEN
                        INSERT (UtilisateurId, FormationId, Note, DateNote)
                        VALUES (source.UtilisateurId, source.FormationId,
                                source.Note, GETDATE());";

                await _db.ExecuteAsync(sql, new
                {
                    UtilisateurId = note.UtilisateurId,
                    FormationId = note.FormationId,
                    Note = note.Note
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la sauvegarde de la note " +
                    $"(Utilisateur #{note.UtilisateurId}, Formation #{note.FormationId}) : {ex.Message}", ex);
            }
        }
    }
}
