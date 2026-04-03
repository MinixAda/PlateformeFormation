
// Infrastructure/Repositories/RoleRepository.cs
//
// Implémentation Dapper du repository des rôles.


using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using PlateformeFormation.Domain.Entities;
using PlateformeFormation.Domain.Interfaces;

namespace PlateformeFormation.Infrastructure.Repositories
{
    //
    // Repository Dapper pour la gestion des rôles utilisateurs.
    // Les rôles (Admin=1, Formateur=2, Apprenant=3) sont des données
    // de référence — normalement fixes, initialisées au démarrage.
    //
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbConnection _db;

        public RoleRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // GetAllAsync
        
        //
        // Retourne tous les rôles disponibles.
        // Utilisé par RoleController (GET /api/Role) et AdminUsersPage
        // pour peupler les listes déroulantes.
        //
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                var sql = "SELECT Id, Nom FROM Role ORDER BY Id;";
                return await _db.QueryAsync<Role>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de la liste des rôles : {ex.Message}", ex);
            }
        }

        
        // GetByNameAsync
        
        //Retourne un rôle par son nom exact. Null si introuvable.</summary>
        public async Task<Role?> GetByNameAsync(string name)
        {
            try
            {
                var sql = "SELECT Id, Nom FROM Role WHERE Nom = @Nom;";
                return await _db.QueryFirstOrDefaultAsync<Role>(sql, new { Nom = name });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la recherche du rôle '{name}' : {ex.Message}", ex);
            }
        }

        
        // GetByIdAsync
        
        //
        // Retourne un rôle par son ID.
        // Utilisé par AuthController après authentification pour récupérer
        // le nom du rôle à inclure dans le token JWT.
        //
        public async Task<Role?> GetByIdAsync(int id)
        {
            try
            {
                var sql = "SELECT Id, Nom FROM Role WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Role>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération du rôle #{id} : {ex.Message}", ex);
            }
        }

        
        // CreateAsync
        
        //Crée un nouveau rôle. Réservé à l'administration.</summary>
        public async Task CreateAsync(Role role)
        {
            try
            {
                var sql = "INSERT INTO Role (Nom) VALUES (@Nom);";
                await _db.ExecuteAsync(sql, role);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création du rôle '{role.Nom}' : {ex.Message}", ex);
            }
        }

        
        // UpdateAsync
        
        //Met à jour le nom d'un rôle existant.</summary>
        public async Task UpdateAsync(Role role)
        {
            try
            {
                var sql = "UPDATE Role SET Nom = @Nom WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, role);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour du rôle #{role.Id} : {ex.Message}", ex);
            }
        }

        
        // DeleteAsync
        
        //Supprime un rôle par son ID.</summary>
        public async Task DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM Role WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression du rôle #{id} : {ex.Message}", ex);
            }
        }

        
        // CreateIfNotExistsAsync
        
        //
        // Crée un rôle uniquement s'il n'existe pas déjà en base.
        // Appelé au démarrage de l'application (Program.cs) pour garantir
        // que Admin, Formateur et Apprenant existent toujours.
        // Utilise IF NOT EXISTS pour être idempotent.
        //
        public async Task CreateIfNotExistsAsync(Role role)
        {
            try
            {
                var sql = @"
                    IF NOT EXISTS (SELECT 1 FROM Role WHERE Id = @Id)
                        INSERT INTO Role (Id, Nom) VALUES (@Id, @Nom);";

                await _db.ExecuteAsync(sql, role);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de l'initialisation du rôle '{role.Nom}' (Id={role.Id}) : {ex.Message}", ex);
            }
        }
    }
}