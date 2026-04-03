
// API/Dtos/InscriptionReadDto.cs

namespace PlateformeFormation.API.Dtos
{
    //
    // DTO renvoyé par GET /api/Inscription/mes-inscriptions.
    // Le frontend charge ensuite les détails de chaque formation
    // via GET /api/Formation/{formationId}.
    //
    public class InscriptionReadDto
    {
        public int Id { get; set; }
        public int FormationId { get; set; }

        //Statut : "EnCours" ou "Terminé".</summary>
        public string Statut { get; set; } = string.Empty;
        public DateTime DateInscription { get; set; }
    }
}