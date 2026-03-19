using System.Data;
using Microsoft.Data.SqlClient;              // ✔️ Provider SQL moderne
using Microsoft.Extensions.Configuration;    // ✔️ Nécessaire pour IConfiguration

namespace PlateformeFormation.Infrastructure.Database;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
