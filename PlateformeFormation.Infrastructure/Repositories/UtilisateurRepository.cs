
// Infrastructure/Repositories/UtilisateurRepository.cs
//
// Implémentation Dapper du repository utilisateur.
//
// CORRECTIONS APPLIQUÉES :
//   - Colonnes explicites dans les SELECT (plus de SELECT *)
//   - UpdateProfilAsync ajouté (Bio + LienPortfolio)
//   - Gestion d'exceptions sur chaque méthode


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
    // Repository Dapper pour la gestion des utilisateurs.
    // Toutes les requêtes SQL sont paramétrées pour prévenir les injections SQL.
    //
    public class UtilisateurRepository : IUtilisateurRepository
    {
        private readonly IDbConnection _db;

        // Colonnes sélectionnées explicitement — plus robuste que SELECT *
        // si la table évolue (nouvelles colonnes, changement d'ordre).
        private const string SelectColumns =
            "Id, Nom, Prenom, Email, MotDePasseHash, RoleId, Bio, LienPortfolio";

        //La connexion SQL est injectée via le conteneur DI (Scoped).</summary>
        public UtilisateurRepository(IDbConnection db)
        {
            _db = db;
        }

        
        // GetAllAsync
        
        //Retourne tous les utilisateurs. Réservé aux administrateurs.</summary>
        public async Task<IEnumerable<Utilisateur>> GetAllAsync()
        {
            try
            {
                var sql = $"SELECT {SelectColumns} FROM Utilisateur ORDER BY Nom, Prenom;";
                return await _db.QueryAsync<Utilisateur>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de la liste des utilisateurs : {ex.Message}", ex);
            }
        }

        
        // GetByEmailAsync
        
        //
        // Retourne un utilisateur par son email.
        // Utilisé par AuthController lors de la connexion et de l'inscription
        // pour vérifier l'unicité de l'email.
        //
        public async Task<Utilisateur?> GetByEmailAsync(string email)
        {
            try
            {
                var sql = $"SELECT {SelectColumns} FROM Utilisateur WHERE Email = @Email;";
                return await _db.QueryFirstOrDefaultAsync<Utilisateur>(sql, new { Email = email });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la recherche de l'utilisateur par email '{email}' : {ex.Message}", ex);
            }
        }

        
        // GetByIdAsync
        
        //Retourne un utilisateur par son ID. Null si introuvable.</summary>
        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            try
            {
                var sql = $"SELECT {SelectColumns} FROM Utilisateur WHERE Id = @Id;";
                return await _db.QueryFirstOrDefaultAsync<Utilisateur>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la récupération de l'utilisateur #{id} : {ex.Message}", ex);
            }
        }

        
        // CreateAsync
        
        //
        // Crée un nouvel utilisateur en base.
        // Le mot de passe DOIT être hashé avant l'appel (via PasswordService).
        // Bio et LienPortfolio sont optionnels à la création.
        //
        public async Task CreateAsync(Utilisateur user)
        {
            try
            {
                var sql = @"
                    INSERT INTO Utilisateur (Nom, Prenom, Email, MotDePasseHash, RoleId, Bio, LienPortfolio)
                    VALUES (@Nom, @Prenom, @Email, @MotDePasseHash, @RoleId, @Bio, @LienPortfolio);";

                await _db.ExecuteAsync(sql, user);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la création de l'utilisateur '{user.Email}' : {ex.Message}", ex);
            }
        }

        
        // UpdateAsync — mise à jour administrative (admin seulement)
        
        //
        // Met à jour les informations administratives d'un utilisateur :
        // Nom, Prenom, Email, RoleId.
        // NE modifie PAS le mot de passe ni les champs de profil (Bio, LienPortfolio).
        // Réservé aux administrateurs via UtilisateursController.
        //
        public async Task UpdateAsync(Utilisateur user)
        {
            try
            {
                var sql = @"
                    UPDATE Utilisateur SET
                        Nom    = @Nom,
                        Prenom = @Prenom,
                        Email  = @Email,
                        RoleId = @RoleId
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, user);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour de l'utilisateur #{user.Id} : {ex.Message}", ex);
            }
        }

        
        // UpdateProfilAsync — mise à jour du profil (utilisateur lui-même)
        
        //
        // Met à jour les champs de profil accessibles par l'utilisateur lui-même :
        // Bio et LienPortfolio (exigé par les consignes TFE).
        // NE modifie PAS Email, RoleId ni le mot de passe.
        // Appelé depuis ProfilPage via PATCH /api/auth/profil.
        //
        public async Task UpdateProfilAsync(Utilisateur user)
        {
            try
            {
                var sql = @"
                    UPDATE Utilisateur SET
                        Bio            = @Bio,
                        LienPortfolio  = @LienPortfolio
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, user);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour du profil de l'utilisateur #{user.Id} : {ex.Message}", ex);
            }
        }

        
        // UpdatePasswordAsync
        
        //
        // Met à jour uniquement le hash du mot de passe.
        // Appelé par POST /api/auth/changer-mot-de-passe après vérification
        // de l'ancien mot de passe.
        //
        public async Task UpdatePasswordAsync(Utilisateur utilisateur)
        {
            try
            {
                var sql = @"
                    UPDATE Utilisateur
                    SET MotDePasseHash = @Hash
                    WHERE Id = @Id;";

                await _db.ExecuteAsync(sql, new
                {
                    Hash = utilisateur.MotDePasseHash,
                    Id = utilisateur.Id
                });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la mise à jour du mot de passe de l'utilisateur #{utilisateur.Id} : {ex.Message}", ex);
            }
        }

        
        // DeleteAsync
        
        //Supprime un utilisateur par son ID. Réservé aux administrateurs.</summary>
        public async Task DeleteAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM Utilisateur WHERE Id = @Id;";
                await _db.ExecuteAsync(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Erreur SQL lors de la suppression de l'utilisateur #{id} : {ex.Message}", ex);
            }
        }
    }
}