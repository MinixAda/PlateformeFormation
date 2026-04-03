
// PlateformeFormation.Domain/Interfaces/IQcmRepository.cs
//
// REMPLACE le fichier existant.
//
// MODIFICATIONS par rapport à l'original :
//   - CreateQuestionAsync : void → Task<int> (retourne l'ID généré)
//   - Ajout GetQcmCompletByModuleIdAsync (requête anti-N+1)


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;
//using PlateformeFormation.Domain.Models;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur les QCM.
    // Implémenté par QcmRepository dans la couche Infrastructure.
    //
    public interface IQcmRepository
    {
        // ---- Questions -----------------------------------------

        //Retourne toutes les questions d'un module, triées par Ordre.</summary>
        Task<IEnumerable<Question>> GetQuestionsByModuleIdAsync(int moduleId);

        //Retourne une question par son ID. Null si introuvable.</summary>
        Task<Question?> GetQuestionByIdAsync(int id);

        //
        // Crée une nouvelle question et retourne son ID généré (SCOPE_IDENTITY).
        // MODIFICATION : était Task (void), devient Task&lt;int&gt; pour récupérer l'ID
        // sans faire un second SELECT après l'INSERT.
        //
        Task<int> CreateQuestionAsync(Question question);

        //Met à jour une question existante.</summary>
        Task UpdateQuestionAsync(Question question);

        //Supprime une question et ses réponses (CASCADE SQL).</summary>
        Task DeleteQuestionAsync(int id);

        // ---- Réponses ------------------------------------------

        //
        // Retourne toutes les réponses d'une question.
        // IMPORTANT : EstCorrecte est chargé ici mais NE DOIT PAS
        // être inclus dans les DTOs renvoyés au frontend.
        //
        Task<IEnumerable<Reponse>> GetReponsesByQuestionIdAsync(int questionId);

        //Crée une nouvelle réponse pour une question.</summary>
        Task CreateReponseAsync(Reponse reponse);

        //Met à jour une réponse existante.</summary>
        Task UpdateReponseAsync(Reponse reponse);

        //Supprime une réponse.</summary>
        Task DeleteReponseAsync(int id);

        // ---- Validation / correction ---------------------------

        //
        // Vérifie si une réponse donnée est la bonne pour une question.
        // Utilisé par ValiderQcm pour calculer le score côté serveur.
        //
        Task<bool> IsReponseCorrecteAsync(int questionId, int reponseId);

        //Retourne le nombre de questions d'un module.</summary>
        Task<int> CountQuestionsAsync(int moduleId);

        // ---- Tentatives ----------------------------------------

        //Enregistre une tentative de QCM avec son score.</summary>
        Task CreateTentativeAsync(TentativeQcm tentative);

        //Retourne l'historique des tentatives d'un utilisateur sur un module.</summary>
        Task<IEnumerable<TentativeQcm>> GetTentativesByUserAndModuleAsync(int userId, int moduleId);

        // ---- Optimisation (anti N+1) ---------------------------

        //
        // Retourne les questions ET leurs réponses en une seule requête SQL
        // via un JOIN Question LEFT JOIN Reponse.
        //
        // Évite le problème N+1 du GET /api/Qcm/{moduleId} qui appelait
        // GetReponsesByQuestionIdAsync() dans une boucle foreach.
        //
        // Utilisé par QcmController.GetQuestions().
        // Implémenté dans QcmRepository avec Dapper multi-mapping.
        //
        Task<IEnumerable<QuestionAvecReponses>> GetQcmCompletByModuleIdAsync(int moduleId);
    }
}
