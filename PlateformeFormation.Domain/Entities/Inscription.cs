
// Domain/Entities/Inscription.cs
//
// Entité représentant l'inscription d'un utilisateur à une formation.
//
// CORRECTION APPLIQUÉE :
//   - Commentaire mis à jour : suppression des anciens statuts
//     "Validee" et "Refusee" qui n'existent plus dans la contrainte
//     SQL CHECK. Les seuls statuts valides sont "EnCours" et "Terminé".


using System;

namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente l'inscription d'un utilisateur à une formation.
    // Une inscription est créée quand un apprenant rejoint une formation.
    // Elle passe automatiquement au statut "Terminé" quand tous
    // les modules de la formation ont été complétés.
    //
    public class Inscription
    {
        //Identifiant unique de l'inscription (clé primaire SQL, IDENTITY).</summary>
        public int Id { get; set; }

        //
        // Identifiant de l'utilisateur inscrit.
        // Clé étrangère vers la table Utilisateur.
        //
        public int UtilisateurId { get; set; }

        //
        // Identifiant de la formation concernée.
        // Clé étrangère vers la table Formation.
        // ON DELETE CASCADE : si la formation est supprimée, ses inscriptions le sont aussi.
        //
        public int FormationId { get; set; }

        //
        // Date et heure de l'inscription.
        // Assignée côté serveur (DateTime.Now) — jamais envoyée par le client.
        //
        public DateTime DateInscription { get; set; }

        //
        // Statut actuel de l'inscription.
        // Valeurs possibles (contrainte CHECK en SQL) :
        //   - "EnCours"  : l'apprenant suit activement la formation (valeur par défaut)
        //   - "Terminé"  : tous les modules ont été complétés
        //
        // Mise à jour automatique par ModuleProgressionController quand
        // HasCompletedAllModulesAsync() retourne true.
        //
        public string Statut { get; set; } = "EnCours";
    }
}