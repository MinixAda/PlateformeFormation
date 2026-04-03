
// Domain/Interfaces/IModuleProgressionRepository.cs
//
// Contrat pour la gestion de la progression des apprenants sur les modules.
// Implémenté par ModuleProgressionRepository dans la couche Infrastructure.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur la progression des modules.
    //
    public interface IModuleProgressionRepository
    {
        //
        // Vérifie si un module est déjà marqué comme terminé pour un utilisateur.
        // Empêche les doublons dans la table ModuleProgression.
        //
        Task<bool> IsModuleCompletedAsync(int userId, int moduleId);

        //
        // Enregistre un module comme terminé pour un utilisateur.
        // Insère une ligne dans ModuleProgression avec DateCompletion = GETDATE().
        //
        Task CompleteModuleAsync(int userId, int moduleId);

        //
        // Retourne la liste des modules terminés par un utilisateur pour une formation.
        // Utilisé par la page de progression de l'apprenant.
        //
        Task<IEnumerable<ModuleProgression>> GetProgressionAsync(int userId, int formationId);

        //
        // Vérifie si un utilisateur a terminé TOUS les modules d'une formation.
        // Si true → déclenche le passage de l'inscription en statut "Terminé"
        // et l'émission de l'attestation.
        //
        Task<bool> HasCompletedAllModulesAsync(int userId, int formationId);
    }
}