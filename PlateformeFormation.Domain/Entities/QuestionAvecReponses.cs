using System;
using System.Collections.Generic;
using System.Text;

namespace PlateformeFormation.Domain.Entities
{
    //
    // Modèle d'agrégation regroupant une question et ses réponses.
    // Ce n'est PAS une entité SQL, juste un modèle métier.
    //
    public class QuestionAvecReponses
    {
        public Question Question { get; set; } = new Question();
        // public List<Reponse> Reponses { get; set; } = new();
        public List<Reponse> Reponses { get; set; } = new List<Reponse>();


    }
}
