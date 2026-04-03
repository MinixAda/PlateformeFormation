
// Domain/Entities/Formation.cs
//
// Entité représentant une formation créée par un formateur.
//
// CORRECTION APPLIQUÉE :
//   - Propriété EstPublique ajoutée (était absente dans l'original,
//     ce qui empêchait Dapper de lire/écrire le champ SQL correspondant.
//     La valeur était toujours false, rendant la feature "formation
//     privée" complètement inopérante.)


using System;

namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente une formation créée par un formateur.
    // Contient toutes les métadonnées nécessaires pour l'affichage,
    // la gestion et le filtrage côté frontend.
    //
    public class Formation
    {
        //Identifiant unique de la formation (clé primaire SQL).</summary>
        public int Id { get; set; }

        //
        // Titre de la formation — obligatoire.
        // Affiché dans les listes et les pages de détail.
        //
        public string Titre { get; set; } = string.Empty;

        //Description longue du contenu et des objectifs de la formation.</summary>
        public string? Description { get; set; }

        //
        // Date de création de la formation.
        // Assignée côté serveur (DateTime.Now) lors du POST — jamais envoyée par le client.
        //
        public DateTime DateCreation { get; set; }

        //
        // Identifiant de l'utilisateur (Formateur ou Admin) qui a créé la formation.
        // Clé étrangère vers la table Utilisateur.
        //
        public int CreateurId { get; set; }

        //
        // Type de média principal utilisé dans la formation.
        // Valeurs attendues : Vidéo, PDF, QCM, Lab, Mixte.
        //
        public string? MediaType { get; set; }

        //
        // Mode de diffusion de la formation.
        // Valeurs attendues : Présentiel, Distanciel, Hybride.
        //
        public string? ModeDiffusion { get; set; }

        //
        // Langue de la formation.
        // Valeurs attendues : FR, EN, NL, DE, ES.
        // Utilisé pour le filtre de recherche côté frontend.
        //
        public string? Langue { get; set; }

        //
        // Niveau de difficulté de la formation.
        // Valeurs attendues : Débutant, Intermédiaire, Avancé.
        // Utilisé pour le filtre de recherche côté frontend.
        //
        public string? Niveau { get; set; }

        //
        // Texte libre décrivant les prérequis nécessaires avant de suivre la formation.
        // Distinct des prérequis "structurés" gérés via la table FormationPrerequis.
        //
        public string? Prerequis { get; set; }

        //URL d'une image de couverture affichée dans les cartes et pages de détail.</summary>
        public string? ImageUrl { get; set; }

        //
        // Durée totale estimée de la formation en minutes.
        // Utilisé pour le filtre de recherche par durée côté frontend.
        //
        public int? DureeMinutes { get; set; }

        //
        // Visibilité de la formation.
        // false (défaut) = privée → visible uniquement par le créateur et les admins.
        // true            = publique → visible par tous les visiteurs, même anonymes.
        //
        // CORRECTION : ce champ était absent de l'entité originale alors qu'il existait
        // dans la base SQL, les DTOs et le repository. Dapper ne pouvait pas le mapper.
        //
        public bool EstPublique { get; set; } = false;
    }
}