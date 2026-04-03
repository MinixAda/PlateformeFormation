
// Domain/Interfaces/IInscriptionRepository.cs
//
// Contrat pour la gestion des inscriptions aux formations.
// Implémenté par InscriptionRepository dans la couche Infrastructure.


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur les inscriptions aux formations.
    //
    public interface IInscriptionRepository
    {
        //
        // Vérifie si un utilisateur est déjà inscrit à une formation.
        // Utilisé avant de créer une nouvelle inscription pour éviter les doublons.
        //
        Task<bool> IsAlreadyInscribedAsync(int userId, int formationId);

        //
        // Vérifie si un utilisateur a TERMINÉ une formation (statut = "Terminé").
        // Utilisé par InscriptionController pour vérifier les prérequis avant inscription.
        //
        Task<bool> HasCompletedFormationAsync(int userId, int formationId);

        //Crée une nouvelle inscription en base.</summary>
        Task CreateAsync(Inscription inscription);

        //Retourne toutes les inscriptions d'un utilisateur donné.</summary>
        Task<IEnumerable<Inscription>> GetByUserAsync(int userId);

        //
        // Passe le statut d'une inscription à "Terminé".
        // Appelé automatiquement quand tous les modules d'une formation
        // ont été complétés par l'utilisateur.
        //
        Task MarkAsCompletedAsync(int userId, int formationId);
    }
}