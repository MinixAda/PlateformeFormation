using System;
using System.Collections.Generic;
using System.Text;


namespace PlateformeFormation.Domain.Entities;

// Représente un rôle possible dans le système
public class Role
{
    public int Id { get; set; }      // Clef primaire
    public string Nom { get; set; } = string.Empty; // "Admin", "Formateur", "Apprenant"
}
