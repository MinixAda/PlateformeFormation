
// ÉTAPE 3 (suite)
// Fichier : PlateformeFormation.Domain/Interfaces/ICommentaireRepository.cs


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour les opérations sur les commentaires.
    // Un commentaire peut cibler une Formation ou un Formateur.
    // Implémenté par CommentaireRepository dans Infrastructure.
    //
    public interface ICommentaireRepository
    {
        //
        // Retourne les commentaires visibles d'une formation, triés du plus récent au plus ancien.
        //
        Task<IEnumerable<Commentaire>> GetByFormationAsync(int formationId);

        //
        // Retourne les commentaires visibles laissés sur un formateur, triés du plus récent au plus ancien.
        //
        Task<IEnumerable<Commentaire>> GetByFormateurAsync(int formateurId);

        //Retourne un commentaire par son ID. Null si inexistant.</summary>
        Task<Commentaire?> GetByIdAsync(int id);

        //Crée un nouveau commentaire. Retourne l'ID généré.</summary>
        Task<int> CreateAsync(Commentaire commentaire);

        //Supprime un commentaire (uniquement par l'auteur ou un admin).</summary>
        Task DeleteAsync(int id);

        //
        // Masque ou rend visible un commentaire (action admin après traitement d'un signalement).
        //
        Task SetVisibiliteAsync(int id, bool estVisible);
    }
}
