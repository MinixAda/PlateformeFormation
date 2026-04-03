
// PlateformeFormation.API/Dtos/NoteFormationResumeDto.cs
//
// FICHIER MANQUANT — NoteFormationResumeDto était dans NoteFormationResumeDto.cs
// mais le fichier original ne contenait que NoteFormationCreateDto.
//
// Ce DTO est retourné par GET /api/NoteFormation/{formationId}.


namespace PlateformeFormation.API.Dtos
{
    //
    // Résumé des notes pour une formation.
    // Retourné par GET /api/NoteFormation/{formationId}.
    // Affiché sur les pages de liste et de détail des formations.
    //
    // Exigence TFE : "Noter (4.5/5) une formation"
    //
    public class NoteFormationResumeDto
    {
        //ID de la formation.</summary>
        public int FormationId { get; set; }

        //
        // Moyenne des notes (arrondie à 1 décimale, ex : 4.3).
        // Null si aucune note n'a encore été soumise.
        //
        public decimal? Moyenne { get; set; }

        //Nombre total de notes soumises pour cette formation.</summary>
        public int NombreNotes { get; set; }
    }
}
