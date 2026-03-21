using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlateformeFormation.API.Middleware;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Database;
using PlateformeFormation.Infrastructure.Repositories;
using PlateformeFormation.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);



// 1) Configuration des services et de l'injection de dépendances
// Ajoute le support MVC / API Controllers.
// Toutes les routes définies dans les controllers seront automatiquement mappées.

builder.Services.AddControllers();



// 2) Swagger + Support JWT dans Swagger 
// Permet d'exposer la documentation API + d'envoyer un token JWT dans Swagger.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plateforme Formation API",
        Version = "v1"
    });

    // Permet d'envoyer un token JWT dans Swagger via le bouton "Authorize"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Entrer: Bearer {votre_token_jwt}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    // Oblige Swagger à utiliser le token pour les endpoints protégés
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// 3) Fabrication de la connexion SQL
// DbConnectionFactory encapsule la logique de création de connexions SQL.
// On l'enregistre en Singleton car elle ne contient pas d'état.

builder.Services.AddSingleton<DbConnectionFactory>();

// 4) Identifiantion de la connexion SQL pour Dapper

// Chaque requête HTTP obtient sa propre connexion SQL.
// Dapper utilisera cette connexion pour exécuter les requêtes.

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var factory = sp.GetRequiredService<DbConnectionFactory>();
    return factory.CreateConnection();
});

// 5) Repositories
// Tous les repositories utilisés dans le projet sont enregistrés ici.
// Chaque requête HTTP obtient sa propre instance.

builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IFormationRepository, FormationRepository>();
builder.Services.AddScoped<IFormationPrerequisRepository, FormationPrerequisRepository>();
builder.Services.AddScoped<IInscriptionRepository, InscriptionRepository>();
builder.Services.AddScoped<IModuleProgressionRepository, ModuleProgressionRepository>();


// 6) Services métiers (PasswordService, JwtService, etc.)
// PasswordService utilise BCrypt → stateless → Singleton OK.

builder.Services.AddSingleton<PasswordService>();

// JwtService dépend de IConfiguration → Scoped ou Singleton possible.
// On choisit Scoped pour rester cohérent avec les repositories.

builder.Services.AddScoped<JwtService>();

// 7) Configuration JWT
// Lecture des paramètres depuis appsettings.json.
// Vérification obligatoire pour éviter les erreurs silencieuses.
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("La clef JWT 'Jwt:Key' est manquante dans appsettings.json");

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new Exception("Le paramètre 'Jwt:Issuer' est manquant dans appsettings.json");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new Exception("Le paramètre 'Jwt:Audience' est manquant dans appsettings.json");


// Active l'authentification JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // En dev, Swagger utilise HTTP → pas besoin de HTTPS obligatoire
        options.RequireHttpsMetadata = false;

        // Permet de récupérer le token via HttpContext
        options.SaveToken = true;

        // Paramètres de validation du token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            // Pas de délai de tolérance → expiration stricte
            ClockSkew = TimeSpan.Zero
        };
    });

// 8) Autorisation
// Active le système d'autorisations basé sur les rôles et les claims.

builder.Services.AddAuthorization();

// Build de l'application avec tous les services et configurations définis ci-dessus.

var app = builder.Build();

// 9) Middleware de gestion des erreurs
// Intercepte toutes les exceptions non gérées et renvoie une réponse JSON propre

app.UseMiddleware<ErrorHandlingMiddleware>();

// 10) Swagger uniquement en développement pour éviter d'exposer la documentation en production

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plateforme Formation API v1");
    });
}

// 11) Redirection HTTP → HTTPS

app.UseHttpsRedirection();

// 12) Authentification et Autorisation

app.UseAuthentication();
app.UseAuthorization();

// 13) Redirection de la racine vers Swagger pour faciliter les tests en développement

app.MapGet("/", () => Results.Redirect("/swagger"));

// 14) Mapping des controllers

app.MapControllers();

// 15) Lancement de l'application

app.Run();
