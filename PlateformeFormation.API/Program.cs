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

/* 
   1) Controllers(API)
    */

builder.Services.AddControllers();

/* 
   2) Swagger + support JWT
    */

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Plateforme Formation API",
        Version = "v1"
    });

    // Ajout du bouton "Authorize" pour JWT
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

/* 
   3) DB (Dapper)
    */

builder.Services.AddSingleton<DbConnectionFactory>();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var factory = sp.GetRequiredService<DbConnectionFactory>();
    return factory.CreateConnection();
});

/* 
   4) Repositories
    */

builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IFormationRepository, FormationRepository>();
builder.Services.AddScoped<IFormationPrerequisRepository, FormationPrerequisRepository>();
builder.Services.AddScoped<IInscriptionRepository, InscriptionRepository>();
builder.Services.AddScoped<IModuleProgressionRepository, ModuleProgressionRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>(); // ❗ Correction importante

/* 
   5) Services métiers
    */

builder.Services.AddSingleton<PasswordService>(); // BCrypt
builder.Services.AddScoped<JwtService>();         // JWT

/* 
   6) config. JWT
    */

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("La clef JWT 'Jwt:Key' est manquante dans appsettings.json");

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new Exception("Le paramètre 'Jwt:Issuer' est manquant dans appsettings.json");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new Exception("Le paramètre 'Jwt:Audience' est manquant dans appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
            ClockSkew = TimeSpan.Zero
        };
    });

/* 
   7) autorisation
    */

builder.Services.AddAuthorization();

/* 
   8) CORS — PERMET AU FRONT REACT DE SE CONNECTER
 8) CORS — Permet au front React de se connecter
    */

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // URL du front React
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

/* 
   Build
    */

var app = builder.Build();

/* 
   9) MIDDLEWARE GLOBAL D'ERREURS
Middleware global d'erreurs
    */

app.UseMiddleware<ErrorHandlingMiddleware>();

/* 
   10) Swagger en dev
    */

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Plateforme Formation API v1");
    });
}

/* 
   11) HTTPS
    */

app.UseHttpsRedirection();

/* 
   12) CORS — doit être avant autthentification
    */

app.UseCors("AllowFrontend");

/* 
   13) AUTHENTICATION + AUTHORIZATION
Authentification + autorisations
    */

app.UseAuthentication();
app.UseAuthorization();

/* 
   14) Redirecection racine -->  Swagger
    */

app.MapGet("/", () => Results.Redirect("/swagger"));

/* 
   15) Conrollers
    */

app.MapControllers();

/* 
   16) Run
    */

app.Run();
