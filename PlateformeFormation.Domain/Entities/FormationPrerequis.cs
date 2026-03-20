using System;
using System.Collections.Generic;
using System.Text;
using System;

namespace PlateformeFormation.Domain.Entities
{

    // Représente un lien entre une formation et une formation prérequise.
    // Relation N <--> N entre les formations.

    public class FormationPrerequis
    {
    
        // Identifiant de la formation cible.
    
        public int FormationId { get; set; }

    
        // Identifiant de la formation qui doit être validée avant.
    
        public int FormationRequiseId { get; set; }
    }
}
