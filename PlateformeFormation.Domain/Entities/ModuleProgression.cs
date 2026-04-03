
// Domain/Entities/ModuleProgression.cs
//
// Entité représentant la progression d'un utilisateur sur un module.
// Chaque ligne indique qu'un utilisateur a terminé un module spécifique.


using System;

namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente la complétion d'un module par un utilisateur.
    // Une ligne = un module terminé par un utilisateur à une date donnée.
    // Contrainte UNIQUE (UtilisateurId, ModuleId) en SQL :
    // un module ne peut être enregistré qu'une seule fois par utilisateur.
    //
    public class ModuleProgression
    {
        //Identifiant unique de la progression (clé primaire SQL, IDENTITY).</summary>
        public int Id { get; set; }

        //
        // Identifiant de l'utilisateur ayant terminé le module.
        // Clé étrangère vers la table Utilisateur.
        //
        public int UtilisateurId { get; set; }

        //
        // Identifiant du module terminé.
        // Clé étrangère vers la table Module.
        // ON DELETE CASCADE : si le module est supprimé, sa progression l'est aussi.
        //
        public int ModuleId { get; set; }

        //
        // Date et heure à laquelle le module a été marqué comme terminé.
        // Inséré avec GETDATE() côté SQL dans CompleteModuleAsync() du repository.
        // Affiché sur la page de progression de l'apprenant.
        //
        public DateTime DateCompletion { get; set; }

        //
        // Indicateur de complétion — toujours true dans cette implémentation.
        // Présent pour correspondre au schéma SQL et permettre une évolution future
        // (ex : annulation d'une complétion par un admin).
        //
        public bool EstTermine { get; set; }
    }
}