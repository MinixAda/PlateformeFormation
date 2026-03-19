using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlateformeFormation.Domain.Interfaces;
using PlateformeFormation.Infrastructure.Database;
using PlateformeFormation.Infrastructure.Repositories;
using PlateformeFormation.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


// 1) Ajoute les controllers

builder.Services.AddControllers();


// 2) Swagger + support JWT dans Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plateforme Formation API",
        Version = "v1"
    });

    // Permet d'envoyer un token JWT dans Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Entrer: Bearer {votre_token_jwt}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
            new string[] {}
        }
    });
});


// 3) Enregistre la fabrique de connexion SQL

builder.Services.AddSingleton<DbConnectionFactory>();


// 4) Enregistre une connexion IDbConnection par requête HTTP

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var factory = sp.GetRequiredService<DbConnectionFactory>();
    return factory.CreateConnection();
});


// 5) Enregistrer les repositories

builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IFormationRepository, FormationRepository>();
//builder.Services.AddScoped<IFormationRepository, FormationRepository>();



// 6) Enregistre le service de mot de passe (BCrypt)

builder.Services.AddSingleton<PasswordService>();


// 7) Configuration JWT

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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


// 8) Active l'autorisation

builder.Services.AddAuthorization();

var app = builder.Build();


// 9) Active Swagger en développement

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plateforme Formation API v1");
    });
}

app.UseHttpsRedirection();


// 10) Active l'authentification + autorisation

app.UseAuthentication();
app.UseAuthorization();

// Redirection automatique vers Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));


// 11) Mapping des controllers

app.MapControllers();


// 12) Lance l'application

app.Run();
