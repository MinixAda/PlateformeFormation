/*
 * using System.Data;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories;

// Implémentation Dapper du repository utilisateur
public class UtilisateurRepository : IUtilisateurRepository
{
    private readonly IDbConnection _connection;

    public UtilisateurRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Utilisateur?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM Utilisateur WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<Utilisateur>(sql, new { Id = id });
    }

    public async Task<Utilisateur?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM Utilisateur WHERE Email = @Email";
        return await _connection.QueryFirstOrDefaultAsync<Utilisateur>(sql, new { Email = email });
    }

    public async Task<IEnumerable<Utilisateur>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Utilisateur";
        return await _connection.QueryAsync<Utilisateur>(sql);
    }

    public async Task<int> CreateAsync(Utilisateur user)
    {
        const string sql = @"
            INSERT INTO Utilisateur (Nom, Prenom, Email, MotDePasseHash, Role, Bio, LienPortfolio, Actif)
            VALUES (@Nom, @Prenom, @Email, @MotDePasseHash, @Role, @Bio, @LienPortfolio, @Actif);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
        ";

        return await _connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<bool> UpdateAsync(Utilisateur user)
    {
        const string sql = @"
            UPDATE Utilisateur
            SET Nom = @Nom,
                Prenom = @Prenom,
                Email = @Email,
                MotDePasseHash = @MotDePasseHash,
                Role = @Role,
                Bio = @Bio,
                LienPortfolio = @LienPortfolio,
                Actif = @Actif
            WHERE Id = @Id;
        ";

        var rows = await _connection.ExecuteAsync(sql, user);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Utilisateur WHERE Id = @Id";
        var rows = await _connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}


*/