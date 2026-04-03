
// PlateformeFormation.Infrastructure/Repositories/QcmRepository.cs
//
// Repository Dapper pour la gestion complète des QCM :
//   - Questions (CRUD)
//   - Réponses (CRUD)
//   - Validation (correction côté serveur)
//   - Tentatives (historique)
//   - Chargement complet anti-N+1 (multi-mapping Dapper)
//
//  Règle Dapper (!!! pour la maintenance) :
//   Les noms de colonnes SQL doivent correspondre EXACTEMENT aux noms
//   des propriétés C# de l'entité cible. Un alias change ce nom et
//   casse le mapping silencieusement (pas d'exception, juste valeur nulle).


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    //
    // Implémentation Dapper du repository QCM.
    //
    // Gère les questions, les réponses et les tentatives de QCM.
    // La correction des réponses se fait uniquement côté serveur
    // (EstCorrecte n'est jamais envoyé au frontend).
    //
    // Dépendance : IDbConnection injectée via le conteneur DI (Scoped).
    // Une connexion est ouverte/fermée automatiquement par Dapper
    // à chaque appel de QueryAsync / ExecuteAsync.
    //
    public class QcmRepository : IQcmRepository
    {
        // Connexion SQL Server injectée par le conteneur DI.
        // Scoped = une connexion par requête HTTP → pas de partage entre requêtes parallèles.
        private readonly IDbConnection _db;

        //
        // Constructeur — reçoit la connexion SQL via injection de dépendances.
        // Lève ArgumentNullException immédiatement si la connexion est null
        // (fail-fast : détecte les oublis d'enregistrement DI au démarrage).
        //
        public QcmRepository(IDbConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db),
                "La connexion IDbConnection ne peut pas être null. " +
                "Vérifiez l'enregistrement dans Program.cs.");
        }

        
        // 1) Questions 
        // Opérations CRUD sur les questions d'un module.
        // Retourne toutes les questions d'un module, triées par leur ordre d'affichage.
        // Utilisé par QcmController.ValiderQcm() pour la correction côté serveur.
        // Note : cette méthode ne charge pas les réponses.
        // Pour les questions + réponses en une requête, utiliser GetQcmCompletByModuleIdAsync().
        // <param name="moduleId">Identifiant du module dont on veut les questions.</param>
        // <returns>Liste des questions (sans leurs réponses), triées par Ordre.</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL avec un message explicite.</exception>
        public async Task<IEnumerable<Question>> GetQuestionsByModuleIdAsync(int moduleId)
        {
            try
            {
                var sql = @"
                    SELECT Id, ModuleId, Texte, Ordre
                    FROM   Question
                    WHERE  ModuleId = @ModuleId
                    ORDER  BY Ordre ASC;";

                return await _db.QueryAsync<Question>(sql, new { ModuleId = moduleId });
            }
            catch (Exception ex)
            {
                // On encapsule pour ajouter le contexte (moduleId) au message d'erreur,
                // ce qui facilite le diagnostic dans les logs sans exposer la stack trace
                // à la couche supérieure.
                throw new Exception(
                    $"Erreur SQL lors de la récupération des questions " +
                    $"du module #{moduleId} : {ex.Message}", ex);
            }
        }
        // Retourne une question par son identifiant.
        // Utilisé par QcmController.UpdateQuestion() et DeleteQuestion()
        // pour vérifier l'existence avant de modifier ou supprimer.
        // <param name="id">Identifiant de la question.</param>
        // <returns>La question si trouvée, null sinon.</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            try
            {
                const string sql = "SELECT Id, ModuleId, Texte, Ordre FROM Question WHERE Id = @Id;";

                return await _db.QueryFirstOrDefaultAsync<Question>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de la question #{id} : {ex.Message}", ex);
            }
        }

        //
        // Insère une nouvelle question en base et retourne son identifiant généré.
        //
        // SCOPE_IDENTITY() retourne l'Id auto-incrémenté de la dernière insertion
        // dans la session SQL courante — thread-safe contrairement à @@IDENTITY.
        //
        // <param name="question">Question à créer (ModuleId, Texte, Ordre doivent être renseignés).</param>
        // <returns>L'Id généré par SQL Server (IDENTITY).</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task<int> CreateQuestionAsync(Question question)
        {
            try
            {
                // Deux instructions dans une même chaîne SQL :
                //   1. INSERT → insère la question
                //   2. SELECT CAST(SCOPE_IDENTITY() AS INT) → retourne l'Id généré
                // ExecuteScalarAsync<int> exécute les deux et retourne la valeur scalaire.
                var sql = @"
                    INSERT INTO Question (ModuleId, Texte, Ordre)
                    VALUES (@ModuleId, @Texte, @Ordre);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                return await _db.ExecuteScalarAsync<int>(sql, question);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création d'une question " +
                    $"pour le module #{question.ModuleId} : {ex.Message}", ex);
            }
        }

        //
        // Met à jour le texte et/ou l'ordre d'une question existante.
        // Les réponses liées ne sont PAS affectées.
        //
        // Pour modifier les réponses d'une question, utiliser les méthodes
        // UpdateReponseAsync() ou supprimer/recréer la question entière.
        //
        // <param name="question">Question avec les nouvelles valeurs (Id requis pour le WHERE).</param>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task UpdateQuestionAsync(Question question)
        {
            try
            {
                var sql = @"
                    UPDATE Question
                    SET    Texte = @Texte,
                           Ordre = @Ordre
                    WHERE  Id    = @Id;";

                await _db.ExecuteAsync(sql, question);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour de la question #{question.Id} : {ex.Message}", ex);
            }
        }

        //
        // Supprime une question et toutes ses réponses associées.
        //
        // La suppression en cascade des réponses est assurée par la contrainte SQL :
        //   CONSTRAINT FK_Reponse_Question FOREIGN KEY (QuestionId)
        //   REFERENCES Question(Id) ON DELETE CASCADE
        //
        // Aucune requête supplémentaire n'est nécessaire pour les réponses.
        //
        // <param name="id">Identifiant de la question à supprimer.</param>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task DeleteQuestionAsync(int id)
        {
            try
            {
                // ON DELETE CASCADE dans le DDL SQL supprime automatiquement
                // toutes les réponses de cette question.
                await _db.ExecuteAsync(
                    "DELETE FROM Question WHERE Id = @Id;",
                    new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression de la question #{id} : {ex.Message}", ex);
            }
        }

        
        // 2) Réponses
        // Opérations CRUD sur les réponses d'une question.
            // Retourne toutes les réponses d'une question donnée.
            // Sécurité: EstCorrecte est chargé ici (nécessaire en interne)
        // mais ne dt pas être exposé dans les DTOs envoyés au frontend.
        // Seul IsReponseCorrecteAsync() l'utilise pour la correction serveur.
         // <param name="questionId">Identifiant de la question parente.</param>
        // <returns>Liste des réponses avec leur indicateur de correction.</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task<IEnumerable<Reponse>> GetReponsesByQuestionIdAsync(int questionId)
        {
            try
            {
                var sql = @"
                    SELECT Id, QuestionId, Texte, EstCorrecte
                    FROM   Reponse
                    WHERE  QuestionId = @QuestionId;";

                return await _db.QueryAsync<Reponse>(sql, new { QuestionId = questionId });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération des réponses " +
                    $"de la question #{questionId} : {ex.Message}", ex);
            }
        }

        // Insère une nouvelle réponse liée à une question.
        // Appelé par QcmController.CreateQuestion() en boucle pour chaque réponse.
        // Règle métier appliquée dans le controller (not here) :
        // Exactement 1 réponse avec EstCorrecte = true par question.
        // <param name="reponse">Réponse à créer (QuestionId, Texte, EstCorrecte requis).</param>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
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
                    $"Erreur SQL lors de la création d'une réponse " +
                    $"pour la question #{reponse.QuestionId} : {ex.Message}", ex);
            }
        }

        // Met à jour le texte et/ou l'indicateur de correction d'une réponse.
        // <param name="reponse">Réponse avec les nouvelles valeurs (Id requis pour le WHERE).</param>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task UpdateReponseAsync(Reponse reponse)
        {
            try
            {
                var sql = @"
                    UPDATE Reponse
                    SET    Texte       = @Texte,
                           EstCorrecte = @EstCorrecte
                    WHERE  Id          = @Id;";

                await _db.ExecuteAsync(sql, reponse);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour de la réponse #{reponse.Id} : {ex.Message}", ex);
            }
        }


        // Supprime une réponse par son identifiant.
        //
        // <param name="id">Identifiant de la réponse à supprimer.</param>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task DeleteReponseAsync(int id)
        {
            try
            {
                await _db.ExecuteAsync(
                    "DELETE FROM Reponse WHERE Id = @Id;",
                    new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression de la réponse #{id} : {ex.Message}", ex);
            }
        }

        
        // 3) Validation et utilitaires
        

        //
        // Vérifie côté serveur si une réponse donnée est la bonne réponse
        // pour une question donnée.
        //
        // Sécurité : c'est ici que se fait la correction — jamais côté client.
        // Le frontend envoie les IDs des réponses choisies, et ce sont ces IDs
        // qui sont vérifiés en base contre EstCorrecte = 1.
        //
        // La double condition (Id ET QuestionId) empêche une attaque où
        // un utilisateur enverrait l'Id d'une réponse correcte d'une autre question.
        //
        // <param name="questionId">Id de la question à valider.</param>
        // <param name="reponseId">Id de la réponse soumise par l'apprenant.</param>
        // <returns>true si la réponse est correcte pour cette question, false sinon.</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task<bool> IsReponseCorrecteAsync(int questionId, int reponseId)
        {
            try
            {
                var sql = @"
                    SELECT COUNT(*)
                    FROM   Reponse
                    WHERE  Id          = @ReponseId
                      AND  QuestionId  = @QuestionId
                      AND  EstCorrecte = 1;";

                // COUNT(*) retourne 0 ou 1 → conversion directe en bool
                int count = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    ReponseId = reponseId,
                    QuestionId = questionId
                });

                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la validation de la réponse #{reponseId} " +
                    $"pour la question #{questionId} : {ex.Message}", ex);
            }
        }

        // Retourne le nombre de questions enregistrées pour un module.
        // Utilisé par CreateQuestionAsync() pour calculer automatiquement
        // l'ordre de la nouvelle question si aucun ordre n'est fourni.
        // <param name="moduleId">Identifiant du module.</param>
        // <returns>Nombre de questions existantes pour ce module.</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
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
                    $"Erreur SQL lors du comptage des questions " +
                    $"du module #{moduleId} : {ex.Message}", ex);
            }
        }

        
        // 4) Tentatives
        // Historique des passages de QCM.
        // Enregistre une tentative de QCM avec son score.
        // Appelé par QcmController.ValiderQcm() après chaque soumission,
        // qu'elle soit réussie ou non.
        // Un apprenant peut faire plusieurs tentatives sur le même module
        // (aucune contrainte UNIQUE sur (UtilisateurId, ModuleId) dans TentativeQcm).
        // <param name="tentative">Tentative avec UserId, ModuleId, Score, Total, Reussi, Date.</param>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
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
                    $"Erreur SQL lors de l'enregistrement de la tentative QCM " +
                    $"(Module #{tentative.ModuleId}, Utilisateur #{tentative.UtilisateurId}) : {ex.Message}", ex);
            }
        }

        // Retourne l'historique des tentatives d'un utilisateur sur un module spécifique.
        // Triées de la plus récente à la plus ancienne (ORDER BY DateTentative DESC).
        // Utilisé par QcmPage.tsx pour afficher l'historique après soumission.
        // <param name="userId">Identifiant de l'utilisateur.</param>
        // <param name="moduleId">Identifiant du module.</param>
        // <returns>Liste des tentatives, triées par date décroissante.</returns>
        // <exception cref="Exception">Encapsule toute erreur SQL.</exception>
        public async Task<IEnumerable<TentativeQcm>> GetTentativesByUserAndModuleAsync(
            int userId,
            int moduleId)
        {
            try
            {
                var sql = @"
                    SELECT Id, UtilisateurId, ModuleId, Score, Total, Reussi, DateTentative
                    FROM   TentativeQcm
                    WHERE  UtilisateurId = @UserId
                      AND  ModuleId      = @ModuleId
                    ORDER  BY DateTentative DESC;";

                return await _db.QueryAsync<TentativeQcm>(sql, new
                {
                    UserId = userId,
                    ModuleId = moduleId
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de l'historique QCM " +
                    $"(Utilisateur #{userId}, Module #{moduleId}) : {ex.Message}", ex);
            }
        }

        
        // 5) — Qcm complet (anti N+1)
        // Charge questions + réponses en 1 seule requête SQL.   
        // Cette méthode existe pcq :
        //   Sans elle, charger 8 questions avec 4 réponses chacune = 9 requêtes SQL
        //   (1 pour les questions + 1 par question pour ses réponses).
        //   Avec cette méthode = 1 seule requête → gain de performance majeur.
        //
        // Multi-mapping Dapper :
        //   QueryAsync<T1, T2, TResult>(sql, mapping, params, splitOn: "colonne")
        //
        //   1. Dapper exécute la requête SQL → obtient N lignes aplaties
        //      (une ligne par combinaison question+réponse)
        //   2. Pour chaque ligne, il crée un objet T1 (Question) ET un objet T2 (Reponse)
        //      en utilisant splitOn pour savoir où couper la ligne entre les deux objets.
        //   3. Le lambda (question, reponse) => ... reçoit les deux objets et
        //      construit le résultat (ici : regroupement dans un dictionnaire).
        //
        //   splitOn: "Id" signifie : "commence le 2ème objet au 2ème champ nommé 'Id'"
        //   Dans notre SELECT :
        //     q.Id, q.ModuleId, q.Texte, q.Ordre → mappés sur Question
        //     r.Id, r.QuestionId, r.Texte, r.EstCorrecte → mappés sur Reponse
        //
        // RÈGLE CRITIQUE : les noms de colonnes SQL doivent correspondre EXACTEMENT
        //   aux noms des propriétés C# de l'entité cible.
        //   Un alias (r.Id AS ReponseId) change ce nom et casse silencieusement le mapping.
        

        //
        // Charge toutes les questions d'un module avec leurs réponses en une seule requête SQL.
        //
        // Algorithme de regroupement (dictionnaire) :
        //   - La requête retourne une ligne par combinaison (question, réponse).
        //   - Pour une question avec 4 réponses → 4 lignes avec le même q.Id.
        //   - On regroupe en un seul objet QuestionAvecReponses par question
        //     grâce au dictionnaire (clef = question.Id).
        //
        // LEFT JOIN : utilisé pour que les questions sans réponse soient quand même
        //   retournées (avec r.* = NULL). La guard (reponse?.Id > 0) filtre ces lignes vides.
        //
        // <param name="moduleId">Identifiant du module dont on veut le QCM complet.</param>
        // <returns>
        // Collection de QuestionAvecReponses, chaque élément contenant une question
        // et la liste de ses réponses possibles.
        // </returns>
        // <exception cref="Exception">Encapsule toute erreur SQL ou de mapping.</exception>
        public async Task<IEnumerable<QuestionAvecReponses>> GetQcmCompletByModuleIdAsync(int moduleId)
        {
            try
            {
                // --------------------------------------------------------
                // Requête SQL — points importants :
                //
                //    r.Id SANS alias   → Dapper mappe correctement sur Reponse.Id
                //    r.Texte SANS alias → Dapper mappe correctement sur Reponse.Texte
                //    Colonnes dans le bon ordre : Question en premier, Reponse après
                //    LEFT JOIN → retourne aussi les questions sans réponses
                //    ORDER BY q.Ordre, r.Id → affichage ordonné + regroupement stable
                //
                //    NE PAS écrire r.Id AS ReponseId → Dapper cherche Reponse.ReponseId = introuvable
                //    NE PAS écrire r.Texte AS ReponseTexte → Dapper cherche Reponse.ReponseTexte = introuvable
                // --------------------------------------------------------
                var sql = @"
                    SELECT
                        q.Id,           -- → Question.Id
                        q.ModuleId,     -- → Question.ModuleId
                        q.Texte,        -- → Question.Texte
                        q.Ordre,        -- → Question.Ordre
                        r.Id,           -- → Reponse.Id     (splitOn coupe ici)
                        r.QuestionId,   -- → Reponse.QuestionId
                        r.Texte,        -- → Reponse.Texte
                        r.EstCorrecte   -- → Reponse.EstCorrecte
                    FROM  Question q
                    LEFT  JOIN Reponse r ON r.QuestionId = q.Id
                    WHERE q.ModuleId = @ModuleId
                    ORDER BY q.Ordre ASC, r.Id ASC;";

                // Dictionnaire de regroupement : question.Id → QuestionAvecReponses
                // Permet d'accumuler les réponses ligne par ligne sans doublon de question.
                var dict = new Dictionary<int, QuestionAvecReponses>();

                await _db.QueryAsync<Question, Reponse, QuestionAvecReponses>(
                    sql,

                    // Lambda appelé par Dapper pour chaque ligne du résultat SQL.
                    // Reçoit un objet Question ET un objet Reponse déjà mappés.
                    (question, reponse) =>
                    {
                        // ── Étape 1 : récupérer ou créer le conteneur de cette question ──
                        if (!dict.TryGetValue(question.Id, out var qAvecReponses))
                        {
                            // Première fois qu'on voit cette question → créer son conteneur
                            qAvecReponses = new QuestionAvecReponses
                            {
                                Question = question,
                                Reponses = new List<Reponse>()
                            };
                            dict[question.Id] = qAvecReponses;
                        }

                        // ── Étape 2 : ajouter la réponse si elle est valide ──
                        //
                        // Correction : guard (reponse?.Id > 0)
                        // Pourquoi : avec un LEFT JOIN, une question sans réponse retourne
                        // une ligne avec r.* = NULL. Dapper crée alors un objet Reponse
                        // avec toutes ses propriétés à leurs valeurs par défaut (Id = 0, Texte = "").
                        // Sans ce guard, cet objet vide serait ajouté à la liste.
                        // Avec le guard : Id = 0 → réponse ignorée → liste vide (correct).
                        if (reponse?.Id > 0)
                        {
                            qAvecReponses.Reponses.Add(reponse);
                        }

                        // Le lambda doit retourner TResult mais ce retour n'est pas utilisé
                        // par Dapper dans notre cas (on gère tout via le dictionnaire).
                        return qAvecReponses;
                    },

                    // Paramètre SQL
                    new { ModuleId = moduleId },

                    // ── splitOn : où Dapper coupe la ligne pour créer le 2ème objet ──
                    //
                    // splitOn: "Id" → Dapper coupe au 2ème champ nommé "Id" dans le SELECT.
                    //   Champs 1-4 (q.Id, q.ModuleId, q.Texte, q.Ordre)  → Question
                    //   Champs 5-8 (r.Id, r.QuestionId, r.Texte, r.EstCorrecte) → Reponse
                    //
                    // Correction : était "ReponseId" (alias supprimé) → devient "Id"
                    splitOn: "Id"
                );

                // Retourner les valeurs du dictionnaire = une QuestionAvecReponses par question,
                // avec la liste de ses réponses complète.
                return dict.Values;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors du chargement du QCM complet " +
                    $"du module #{moduleId} : {ex.Message}", ex);
            }
        }
    }
}