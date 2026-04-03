
using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur les signalements de contenu.
    // Implémenté par SignalementRepository dans Infrastructure.
    //
    public interface ISignalementRepository
    {
        //
        // Retourne tous les signalements en attente de traitement (admin uniquement).
        // Triés du plus récent au plus ancien.
        //
        Task<IEnumerable<Signalement>> GetEnAttenteAsync();

        //
        // Retourne tous les signalements, quel que soit leur statut (admin uniquement).
        // Utile pour l'historique de modération.
        //
        Task<IEnumerable<Signalement>> GetAllAsync();

        //Retourne un signalement par son ID. Null si inexistant.</summary>
        Task<Signalement?> GetByIdAsync(int id);

        //
        // Crée un nouveau signalement. Retourne l'ID généré.
        // Appelé quand un utilisateur clique sur "Signaler".
        //
        Task<int> CreateAsync(Signalement signalement);

        //
        // Met à jour le statut d'un signalement (admin uniquement).
        // Statuts valides : "Traité" | "Rejeté".
        //
        Task UpdateStatutAsync(int id, string statut);
    }
}