
// ÉTAPE 3 — Interfaces Domain
// Fichier : PlateformeFormation.Domain/Interfaces/INoteFormationRepository.cs


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations de notation des formations (1 à 5 étoiles).
    // Implémenté par NoteFormationRepository dans la couche Infrastructure.
    //
    public interface INoteFormationRepository
    {
        //
        // Récupère la note déposée par un utilisateur pour une formation.
        // Retourne null si l'utilisateur n'a pas encore noté cette formation.
        //
        Task<NoteFormation?> GetNoteUtilisateurAsync(int utilisateurId, int formationId);

        //
        // Retourne toutes les notes d'une formation (pour calculer la moyenne).
        //
        Task<IEnumerable<NoteFormation>> GetNotesByFormationAsync(int formationId);

        //
        // Calcule la moyenne des notes d'une formation.
        // Retourne null si aucune note n'a encore été soumise.
        //
        Task<decimal?> GetMoyenneAsync(int formationId);

        //
        // Retourne le nombre de notes soumises pour une formation.
        //
        Task<int> CountNotesAsync(int formationId);

        //
        // Crée ou met à jour la note d'un utilisateur pour une formation.
        // (UPSERT : si déjà noté, on met à jour ; sinon on insère.)
        //
        Task UpsertNoteAsync(NoteFormation note);
    }
}
