
// PlateformeFormation.Infrastructure/Repositories/QcmRepository.cs
//
// Version corrigée et stabilisée.
// - Suppression du using Domain.Models (inexistant)
// - Multi-mapping Dapper corrigé (Question → QuestionAvecReponses)
// - splitOn corrigé (ReponseId)
// - Gestion d’erreurs explicite
// - Commentaires pédagogiques
/*

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
    // Repository QCM basé sur Dapper.
    // Gère les questions, réponses et tentatives.
    //
    public class QcmRepository : IQcmRepository
    {
        private readonly IDbConnection _db;

        public QcmRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        
        // QUESTIONS
        

        public async Task<IEnumerable<Question>> GetQuestionsByModuleIdAsync(int moduleId)
        {
            try
            {
                var sql = @"
SELECT Id, ModuleId, Texte, Ordre
FROM Question
WHERE ModuleId = @ModuleId
ORDER BY Ordre;";

                return await _db.QueryAsync<Question>(sql, new { ModuleId = moduleId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur récupération questions du module #{moduleId} : {ex.Message}", ex);
            }
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            try
            {
                var sql = "SELECT Id, ModuleId, Texte, Ordre FROM Question WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Question>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur récupération question #{id} : {ex.Message}", ex);
            }
        }

        //
        // Crée une question et retourne son ID généré via SCOPE_IDENTITY().
        //
        public async Task<int> CreateQuestionAsync(Question question)
        {
            try
            {
                var sql = @"
INSERT INTO Question (ModuleId, Texte, Ordre)
VALUES (@ModuleId, @Texte, @Ordre);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                return await _db.ExecuteScalarAsync<int>(sql, question);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur création question pour module #{question.ModuleId} : {ex.Message}", ex);
            }
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            try
            {
                var sql = @"
UPDATE Question
SET Texte = @Texte,
    Ordre = @Ordre
WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, question);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur mise à jour question #{question.Id} : {ex.Message}", ex);
            }
        }

        public async Task DeleteQuestionAsync(int id)
        {
            try
            {
                // Les réponses sont supprimées automatiquement via ON DELETE CASCADE
                await _db.ExecuteAsync("DELETE FROM Question WHERE Id = @Id;", new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur suppression question #{id} : {ex.Message}", ex);
            }
        }

        
        // RÉPONSES
        

        public async Task<IEnumerable<Reponse>> GetReponsesByQuestionIdAsync(int questionId)
        {
            try
            {
                var sql = @"
SELECT Id, QuestionId, Texte, EstCorrecte
FROM Reponse
WHERE QuestionId = @QuestionId;";

                return await _db.QueryAsync<Reponse>(sql, new { QuestionId = questionId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur récupération réponses de la question #{questionId} : {ex.Message}", ex);
            }
        }

        public async Task CreateReponseAsync(Reponse reponse)
        {
            try
            {
                var sql = @"
INSERT INTO Reponse (QuestionId, Texte, EstCorrecte)
VALUES (@QuestionId, @Texte, @EstCorrecte);";

                await _db.ExecuteAsync(sql, reponse);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur création réponse pour question #{reponse.QuestionId} : {ex.Message}", ex);
            }
        }

        public async Task UpdateReponseAsync(Reponse reponse)
        {
            try
            {
                var sql = @"
UPDATE Reponse
SET Texte = @Texte,
    EstCorrecte = @EstCorrecte
WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, reponse);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur mise à jour réponse #{reponse.Id} : {ex.Message}", ex);
            }
        }

        public async Task DeleteReponseAsync(int id)
        {
            try
            {
                await _db.ExecuteAsync("DELETE FROM Reponse WHERE Id = @Id;", new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur suppression réponse #{id} : {ex.Message}", ex);
            }
        }

        
        // VALIDATION
        

        public async Task<bool> IsReponseCorrecteAsync(int questionId, int reponseId)
        {
            try
            {
                var sql = @"
SELECT COUNT(*)
FROM Reponse
WHERE Id = @ReponseId
  AND QuestionId = @QuestionId
  AND EstCorrecte = 1;";

                return await _db.ExecuteScalarAsync<int>(sql, new
                {
                    ReponseId = reponseId,
                    QuestionId = questionId
                }) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur validation réponse #{reponseId} / question #{questionId} : {ex.Message}", ex);
            }
        }

        public async Task<int> CountQuestionsAsync(int moduleId)
        {
            try
            {
                return await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM Question WHERE ModuleId = @ModuleId;",
                    new { ModuleId = moduleId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur comptage questions du module #{moduleId} : {ex.Message}", ex);
            }
        }

        
        // TENTATIVES
        

        public async Task CreateTentativeAsync(TentativeQcm tentative)
        {
            try
            {
                var sql = @"
INSERT INTO TentativeQcm
(UtilisateurId, ModuleId, Score, Total, Reussi, DateTentative)
VALUES
(@UtilisateurId, @ModuleId, @Score, @Total, @Reussi, @DateTentative);";

                await _db.ExecuteAsync(sql, tentative);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur enregistrement tentative QCM (module #{tentative.ModuleId}) : {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<TentativeQcm>> GetTentativesByUserAndModuleAsync(int userId, int moduleId)
        {
            try
            {
                var sql = @"
SELECT Id, UtilisateurId, ModuleId, Score, Total, Reussi, DateTentative
FROM TentativeQcm
WHERE UtilisateurId = @UserId
  AND ModuleId = @ModuleId
ORDER BY DateTentative DESC;";

                return await _db.QueryAsync<TentativeQcm>(sql, new
                {
                    UserId = userId,
                    ModuleId = moduleId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur récupération historique QCM (utilisateur #{userId}, module #{moduleId}) : {ex.Message}", ex);
            }
        }

        
        // QCM COMPLET — Anti N+1
        

        //
        // Retourne toutes les questions + leurs réponses en une seule requête.
        // Multi-mapping Dapper : Question → Reponse → regroupement dans QuestionAvecReponses.
        //
        public async Task<IEnumerable<QuestionAvecReponses>> GetQcmCompletByModuleIdAsync(int moduleId)
        {
            try
            {
                var sql = @"
SELECT
    q.Id,
    q.ModuleId,
    q.Texte,
    q.Ordre,
    r.Id AS ReponseId,
    r.QuestionId,
    r.Texte AS ReponseTexte,
    r.EstCorrecte
FROM Question q
LEFT JOIN Reponse r ON r.QuestionId = q.Id
WHERE q.ModuleId = @ModuleId
ORDER BY q.Ordre, r.Id;";

                var dict = new Dictionary<int, QuestionAvecReponses>();

                await _db.QueryAsync<Question, Reponse, QuestionAvecReponses>(
                    sql,
                    (question, reponse) =>
                    {
                        if (!dict.TryGetValue(question.Id, out var q))
                        {
                            q = new QuestionAvecReponses
                            {
                                Question = question,
                                Reponses = new List<Reponse>()
                            };

                            dict[question.Id] = q;
                        }

                        if (reponse != null)
                            q.Reponses.Add(reponse);

                        return q;
                    },
                    new { ModuleId = moduleId },
                    splitOn: "ReponseId"
                );

                return dict.Values;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur chargement QCM complet du module #{moduleId} : {ex.Message}", ex);
            }
        }
    }
}
*/