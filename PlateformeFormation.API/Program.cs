
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;  // Pour Swashbuckle 
using PlateformeFormation.API.Middleware;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Database;
using PlateformeFormation.Infrastructure.Repositories;
using PlateformeFormation.Infrastructure.Services;
using QuestPDF.Infrastructure;
using Resend;                     // Resend pour contact form   
using System.Data;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// QuestPDF : licence Community (obligatoire avant tout appel à des fichiers PDF)
// Sans cette ligne, QuestPDF lève une exception à la première
// génération de PDF (même en dev).
// Community = gratuit pour les projets éducatifs et open-source.
QuestPDF.Settings.License = LicenseType.Community;


// 1) Controllers

builder.Services.AddControllers();


// 2) Swagger — avec support du bouton Authorize (JWT)

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plateforme Formation API",
        Version = "v1",
        Description = "API REST pour la plateforme de formation avec QCM, " +
            "attestations, commentaires, signalements et suivi formateurs."
    });

    // Bouton "Authorize" dans Swagger UI (fonctionne parfaitement avec Swashbuckle 10.1.7)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Veuillez entrer un token JWT valide au format : **Bearer {token}**" +
        "Exemple : Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

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
            Array.Empty<string>()
        }
    });
});


// 3) db Dapper via DbConnectionFactory
// DbConnectionFactory est Singleton (lit la config une fois).
// IDbConnection est Scoped (une connexion par requête HTTP).
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var factory = sp.GetRequiredService<DbConnectionFactory>();
    return factory.CreateConnection();
});


// 4) Repositories — tous Scoped (cycle de vie par requête HTTP)

builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IFormationRepository, FormationRepository>();
builder.Services.AddScoped<IFormationPrerequisRepository, FormationPrerequisRepository>();
builder.Services.AddScoped<IInscriptionRepository, InscriptionRepository>();
builder.Services.AddScoped<IModuleProgressionRepository, ModuleProgressionRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IQcmRepository, QcmRepository>();
//+ nouveau à faire si le tmps
builder.Services.AddScoped<IAttestationRepository, AttestationRepository>();
builder.Services.AddScoped<ICommentaireRepository, CommentaireRepository>();
builder.Services.AddScoped<ISignalementRepository, SignalementRepository>();
builder.Services.AddScoped<ISuiviFormateurRepository, SuiviFormateurRepository>();
builder.Services.AddScoped<INoteFormationRepository, NoteFormationRepository>();

// 5) Services métier

builder.Services.AddSingleton<PasswordService>();
builder.Services.AddScoped<JwtService>();
// + faire AttestationService
// Doit être Scoped (pas Singleton) car il dépend de
// IAttestationRepository, IUtilisateurRepository, IFormationRepository
// qui sont eux-mêmes Scoped.
// Un Singleton ne peut pas dépendre de services Scoped.
builder.Services.AddScoped<AttestationService>();


// 6) Configuration JWT — validation au démarrage

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
// Validation explicite au démarrage — fail-fast si la config est incomplète
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("'Jwt:Key' est manquant dans appsettings.json.");

if (jwtKey.Length < 32)
    throw new InvalidOperationException("'Jwt:Key' doit contenir au moins 32 caractères.");

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("'Jwt:Issuer' est manquant dans appsettings.json.");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("'Jwt:Audience' est manquant dans appsettings.json.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // false en dev (HTTP local), true en production (HTTPS obligatoire)
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            // TimeSpan.Zero = aucune tolérance sur l'expiration du token
            ClockSkew = TimeSpan.Zero
        };
    });


// 7) Autorisation

builder.Services.AddAuthorization();


// 8) Resend — service d'envoi d'emails pour le formulaire de contact
// La clé API est lue depuis les User Secrets (jamais dans appsettings.json)

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiKey"]!;
});
builder.Services.AddTransient<IResend, ResendClient>();


// 9) Cors — autorise le frontend React (Vite sur port 5173)

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Port vite (par défaut)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// Build

var app = builder.Build();


// 10) Initialisation des rôles au démarrage
// idempotent : même effet qu'on l'applique 1 ou plusieur fois

using (var scope = app.Services.CreateScope())
{
    var roleRepo = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
    try
    {
        await roleRepo.CreateIfNotExistsAsync(new Role { Id = 1, Nom = "Admin" });
        await roleRepo.CreateIfNotExistsAsync(new Role { Id = 2, Nom = "Formateur" });
        await roleRepo.CreateIfNotExistsAsync(new Role { Id = 3, Nom = "Apprenant" });
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex,
            "Impossible d'initialiser les rôles. " +
            "Vérifiez que la base 'PlateformeFormation' existe " +
            "et que le script CreateDatabase.sql a été exécuté.");
    }
}


// Pipeline HTTP (ordre critique !)
// 1O) Gestion d'erreurs en 1er --> intercepte toutes les exceptions
app.UseMiddleware<ErrorHandlingMiddleware>();

// 11) Swagger (développement uniquement)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plateforme Formation API v1");
        c.RoutePrefix = "swagger"; // Accessible à /swagger
    });
}

// 12) https redirection (recommandé même en dev pour tester les redirections et les cookies sécurisés)
app.UseHttpsRedirection();

// 13) CORS : doit être avant Authentication
// (un preflight Options doit recevoir les headers CORS avant d'être authentifié)
app.UseCors("AllowFrontend");

// 14) Authentication --> Authorization (impérativement dans cet ordre)
app.UseAuthentication();
app.UseAuthorization();

// 15) Redirection racine "/" vers Swagger (pratique en développement)
app.MapGet("/", () => Results.Redirect("/swagger"));

// 16) mappage controllers
app.MapControllers();


// Démarrage

app.Run();