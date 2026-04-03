
// Infrastructure/Database/DbConnectionFactory.cs
//
// Fabrique responsable de la création des connexions SQL Server.
// Utilisée par Dapper pour exécuter les requêtes paramétrées.


using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PlateformeFormation.Infrastructure.Database
{
    //
    // Fabrique de connexions SQL Server.
    // Enregistrée comme Singleton dans Program.cs — lit la chaîne de
    // connexion une seule fois depuis appsettings.json au démarrage.
    //
    // Chaque requête HTTP reçoit sa propre connexion SQL (Scoped IDbConnection)
    // → pas de partage de connexion entre requêtes parallèles.
    //
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        //
        // Lit la chaîne de connexion "DefaultConnection" depuis appsettings.json.
        // Lève InvalidOperationException si la clé est absente,
        // ce qui fait crasher l'app au démarrage avec un message clair
        // plutôt qu'une erreur SQL obscure plus tard.
        //
        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "La chaîne de connexion 'DefaultConnection' est introuvable dans appsettings.json. " +
                    "Vérifiez que la clé ConnectionStrings:DefaultConnection est bien définie.");
        }

        //
        // Crée et retourne une nouvelle connexion SQL (état = fermée par défaut).
        // Dapper l'ouvre et la ferme automatiquement à chaque requête.
        //
        // Appelé depuis Program.cs dans AddScoped{IDbConnection} :
        //   sp => factory.CreateConnection()
        //
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}