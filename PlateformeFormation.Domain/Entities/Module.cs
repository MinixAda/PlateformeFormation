
// Domain/Entities/Module.cs
//
// Entité représentant un module pédagogique appartenant à une formation.
// Un module est l'unité de base d'une formation (chapitre, leçon, etc.).


namespace PlateformeFormation.Domain.Entities
{
    //
    // Représente un module (unité pédagogique) appartenant à une formation.
    // Les modules sont affichés dans l'ordre défini par la propriété Ordre.
    //
    public class Module
    {
        //Identifiant unique du module (clé primaire SQL, IDENTITY).</summary>
        public int Id { get; set; }

        //
        // Identifiant de la formation parente.
        // Clé étrangère vers la table Formation.
        // ON DELETE CASCADE : si la formation est supprimée, ses modules le sont aussi.
        //
        public int FormationId { get; set; }

        //Titre du module affiché à l'utilisateur.</summary>
        public string Titre { get; set; } = string.Empty;

        //Description détaillée du contenu du module. Nullable — optionnel.</summary>
        public string? Description { get; set; }

        //
        // Ordre d'affichage du module dans la formation.
        // Les modules sont triés par cette valeur dans les requêtes SQL (ORDER BY Ordre).
        //
        public int Ordre { get; set; }

        //Durée estimée du module en minutes. Nullable — peut ne pas être renseignée.</summary>
        public int? DureeMinutes { get; set; }
    }
}