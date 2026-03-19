using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PlateformeFormation.Infrastructure.Repositories
{
    
    //Repository responsable de la gestion des rôles dans la base SQL
    //Utilise Dapper pour exécuter des requêtes rapides et légères
    
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbConnection _db;

        
        //Injection de la connexion SQL (gérée par DI dans Program.cs)
        
        public RoleRepository(IDbConnection db)
        {
            _db = db;
        }

        
        //Récupère tous les rôles présents dans la table Role
        
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            var sql = "SELECT Id, Nom FROM Role;";
            return await _db.QueryAsync<Role>(sql);
        }

        
        //Récupère un rôle par son nom exact
        //Retourne null si aucun rôle ne correspond
        
        public async Task<Role?> GetByNameAsync(string name)
        {
            var sql = "SELECT Id, Nom FROM Role WHERE Nom = @Nom;";
            return await _db.QueryFirstOrDefaultAsync<Role>(sql, new { Nom = name });
        }

        
        //Récupère un rôle par son identifiant
        //Retourne null si aucun rôle ne correspond
        
        public async Task<Role?> GetByIdAsync(int id)
        {
            var sql = "SELECT Id, Nom FROM Role WHERE Id = @Id;";
            return await _db.QueryFirstOrDefaultAsync<Role>(sql, new { Id = id });
        }

        
        //Crée un nouveau rôle dans la la db
        
        public async Task CreateAsync(Role role)
        {
            var sql = "INSERT INTO Role (Nom) VALUES (@Nom);";
            await _db.ExecuteAsync(sql, role);
        }

        
        //Met à jour le nom d'un rôle existant
        
        public async Task UpdateAsync(Role role)
        {
            var sql = "UPDATE Role SET Nom = @Nom WHERE Id = @Id;";
            await _db.ExecuteAsync(sql, role);
        }

        
        //Supprime un rôle par son identifiant
        
        public async Task DeleteAsync(int id)
        {
            var sql = "DELETE FROM Role WHERE Id = @Id;";
            await _db.ExecuteAsync(sql, new { Id = id });
        }

        
        //Crée un rôle uniquement s'il n'existe pas déjà.
        //Utilisé au démarrage de l'application pour initialiser Admin/Formateur/Apprenant.
        
        public async Task CreateIfNotExistsAsync(Role role)
        {
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM Role WHERE Id = @Id)
                INSERT INTO Role (Id, Nom) VALUES (@Id, @Nom);
            ";

            await _db.ExecuteAsync(sql, role);
        }
    }
}
