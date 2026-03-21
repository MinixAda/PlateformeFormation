using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PlateformeFormation.Infrastructure.Database
{
    
    // Fabrique responsable de la création des connexions SQL.
    // Utilisée par Dapper pour exécuter les requêtes.
    
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        
        // Charge la chaîne de connexion depuis appsettings.json.
        
        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in appsettings.json.");
        }

        
        // Crée une nouvelle connexion SQL (fermée par défaut).
        // Chaque requête HTTP obtient sa propre connexion.
        
        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
