// Fichier : PlateformeFormation.Domain/Interfaces/ISuiviFormateurRepository.cs


using System.Collections.Generic;
using System.Threading.Tasks;
using PlateformeFormation.Domain.Entities;

namespace PlateformeFormation.Domain.Interfaces
{
    //
    // Contrat pour la gestion des abonnements apprenant → formateur.
    // Implémenté par SuiviFormateurRepository dans Infrastructure.
    //
    public interface ISuiviFormateurRepository
    {
        //
        // Vérifie si un apprenant suit déjà un formateur donné.
        // Utilisé avant de créer un doublon.
        //
        Task<bool> SuitDejaAsync(int apprenantId, int formateurId);

        //
        // Retourne la liste des IDs des formateurs suivis par un apprenant.
        //
        Task<IEnumerable<int>> GetFormateursIdsSuivisAsync(int apprenantId);

        //
        // Retourne la liste des IDs des apprenants qui suivent un formateur.
        // Utilisé pour envoyer les notifications de nouveautés.
        //
        Task<IEnumerable<int>> GetApprenantIdsSuiveursAsync(int formateurId);

        //
        // Crée un abonnement (apprenant suit formateur).
        // Lève une exception si le lien existe déjà.
        //
        Task SuivreAsync(int apprenantId, int formateurId);

        //
        // Supprime un abonnement (apprenant se désabonne d'un formateur).
        // Retourne false si le lien n'existait pas.
        //
        Task<bool> NePlusSuivreAsync(int apprenantId, int formateurId);
    }
}
