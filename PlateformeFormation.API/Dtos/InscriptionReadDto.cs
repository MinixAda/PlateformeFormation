namespace PlateformeFormation.API.Dtos
{
    public class InscriptionReadDto
    {
        public int Id { get; set; }
        public int FormationId { get; set; }
        public string Statut { get; set; } = string.Empty;
        public DateTime DateInscription { get; set; }
    }
}
