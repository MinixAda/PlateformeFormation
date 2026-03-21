using System.Net;
using System.Text.Json;

namespace PlateformeFormation.API.Middleware
{
    
    // Middleware global chargé d'intercepter TOUTES les exceptions non gérées
    // et de renvoyer une réponse JSON propre, explicite et sécurisée.
    // 
    // Objectifs :
    // - Empêcher les crashs bruts
    // - Standardiser les réponses d'erreur
    // - Masquer les détails sensibles en production
    // - Loguer les erreurs pour diagnostic interne
    
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        
        // Point d'entrée du middleware.
        // Toute requête passe ici AVANT d'atteindre les controllers.
        
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Laisse la requête continuer normalement
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log interne détaillé (jamais envoyé au client)
                _logger.LogError(ex, "❌ Exception non gérée interceptée par ErrorHandlingMiddleware.");

                // Réponse JSON propre et explicite
                await HandleExceptionAsync(context, ex);
            }
        }

        
        // Construit une réponse JSON explicite et sécurisée.
        
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // En dev → détails complets
            // En prod → message générique pour éviter les fuites d'informations
            var response = new
            {
                Status = context.Response.StatusCode,
                Message = "Une erreur interne est survenue lors du traitement de votre requête.",
                Details = _env.IsDevelopment()
                    ? ex.Message
                    : "Une erreur inattendue s'est produite. Si le problème persiste, contactez l'administrateur.",
                ExceptionType = _env.IsDevelopment() ? ex.GetType().Name : null,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
