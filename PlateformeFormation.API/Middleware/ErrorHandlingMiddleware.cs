
// API/Middleware/ErrorHandlingMiddleware.cs
//
// Middleware global d'intercepton des exceptions non gérées.
//
// Objectifs :
//   1. Empêcher les crashs bruts qui renvoient du HTML au lieu de JSON
//   2. Standardiser toutes les réponses d'erreur en JSON
//   3. Logger les détails complets pour le diagnostic interne
//   4. Masquer les informations sensibles en production
//   5. Distinguer les types d'exceptions pour des codes HTTP adaptés


using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlateformeFormation.API.Middleware
{
    //
    // Middleware global chargé d'intercepter toutes les exceptions non gérées
    // et de retourner une réponse JSON cohérente et sécurisée.
    //
    // Enregistré en premier dans le pipeline (Program.cs) pour intercepter
    // toutes les exceptions, y compris celles des autres middlewares.
    //
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        //
        // Point d'entrée du middleware.
        // Toute requête HTTP passe ici avant d'atteindre les controllers.
        //
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Transmet la requête au middleware suivant / controller
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log interne complet (stack trace, type, message)
                // Ce log n'est JAMAIS envoyé au client
                _logger.LogError(ex,
                    "Exception non gérée interceptée — Méthode : {Method} | URL : {Path}",
                    context.Request.Method,
                    context.Request.Path);

                // Construire et envoyer la réponse JSON d'erreur
                await HandleExceptionAsync(context, ex);
            }
        }

        //
        // Construit la réponse JSON d'erreur adaptée selon :
        //   - Le type de l'exception (pour le code HTTP approprié)
        //   - L'environnement (dev = détails complets, prod = message générique)
        //
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Toutes les réponses d'erreur sont en JSON
            context.Response.ContentType = "application/json";

            // Déterminer le code HTTP selon le type d'exception
            int statusCode;
            string messagePublic;

            switch (ex)
            {
                case UnauthorizedAccessException:
                    // Accès refusé (ex : formateur tentant de modifier la formation d'un autre)
                    statusCode = (int)HttpStatusCode.Forbidden;
                    messagePublic = "Accès refusé. Vous n'avez pas les droits nécessaires.";
                    break;

                case ArgumentNullException:
                case ArgumentException:
                    // Données invalides en entrée
                    statusCode = (int)HttpStatusCode.BadRequest;
                    messagePublic = "Données invalides dans la requête.";
                    break;

                case InvalidOperationException:
                    // Opération impossible dans l'état actuel
                    statusCode = (int)HttpStatusCode.Conflict;
                    messagePublic = "Opération impossible dans l'état actuel.";
                    break;

                default:
                    // Erreur interne non typée (SQL, null ref, etc.)
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    messagePublic = "Une erreur interne est survenue.";
                    break;
            }

            context.Response.StatusCode = statusCode;

            // En développement : détails complets pour faciliter le debug
            // En production  : message générique pour ne pas exposer l'architecture interne
            var response = new
            {
                Status = statusCode,
                Message = messagePublic,
                Details = _env.IsDevelopment()
                                    ? ex.Message
                                    : "Si le problème persiste, contactez l'administrateur.",
                ExceptionType = _env.IsDevelopment() ? ex.GetType().Name : null,
                StackTrace = _env.IsDevelopment() ? ex.StackTrace : null,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var options = new JsonSerializerOptions
            {
                // Ignorer les valeurs null (ExceptionType et StackTrace en prod)
                DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            return context.Response.WriteAsync(json);
        }
    }
}