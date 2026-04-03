namespace PlateformeFormation.API.Dtos
{


    
    // ATTESTATION
    

    //
    // DTO renvoyé par GET /api/Attestation et GET /api/Attestation/{formationId}.
    // Contient toutes les informations nécessaires pour afficher l'attestation
    // et proposer le téléchargement du PDF.
    //
    public class AttestationReadDto
    {
        public int Id { get; set; }
        public int FormationId { get; set; }

        //Titre de la formation — joint depuis la table Formation.</summary>
        public string TitreFormation { get; set; } = string.Empty;

        //Niveau de la formation (Débutant, Intermédiaire, Avancé).</summary>
        public string? NiveauFormation { get; set; }

        public DateTime DateObtention { get; set; }
        public string NumeroAttestation { get; set; } = string.Empty;
    }
}