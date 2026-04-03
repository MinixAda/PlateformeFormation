
// Domain/Interfaces/IUtilisateurRepository.cs
//
// Contrat définissant les opérations disponibles sur les utilisateurs.
//
// CORRECTION APPLIQUÉE :
//   - UpdateProfilAsync ajouté pour permettre la mise à jour
//     des champs de profil (Bio, LienPortfolio) sans toucher
//     au mot de passe ni au rôle.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour l'accès aux données des utilisateurs.
    // Implémenté par UtilisateurRepository dans la couche Infrastructure.
    //
    public interface IUtilisateurRepository
    {
        //Retourne un utilisateur par son email. Null si introuvable.</summary>
        Task<Utilisateur?> GetByEmailAsync(string email);

        //Retourne un utilisateur par son ID. Null si introuvable.</summary>
        Task<Utilisateur?> GetByIdAsync(int id);

        //Retourne la liste de tous les utilisateurs.</summary>
        Task<IEnumerable<Utilisateur>> GetAllAsync();

        //
        // Crée un nouvel utilisateur en base.
        // Le mot de passe doit déjà être hashé avant l'appel.
        //
        Task CreateAsync(Utilisateur user);

        //
        // Met à jour les informations administratives d'un utilisateur
        // (Nom, Prenom, Email, RoleId).
        // Réservé aux administrateurs — ne modifie pas le mot de passe.
        //
        Task UpdateAsync(Utilisateur user);

        //
        // Met à jour les informations de profil d'un utilisateur
        // (Bio, LienPortfolio).
        // Accessible à l'utilisateur lui-même depuis sa page de profil.
        //
        Task UpdateProfilAsync(Utilisateur user);

        //
        // Met à jour uniquement le hash du mot de passe.
        // Utilisé par POST /api/auth/changer-mot-de-passe.
        //
        Task UpdatePasswordAsync(Utilisateur utilisateur);

        //Supprime un utilisateur par son ID.</summary>
        Task DeleteAsync(int id);
    }
}