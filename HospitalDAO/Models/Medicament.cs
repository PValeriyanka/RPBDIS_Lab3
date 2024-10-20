using System;
using System.Collections.Generic;

namespace HospitalDAO.Models;

public partial class Medicament
{
    public int MedicamentId { get; set; }

    public string MedicamentName { get; set; } = null!;

    public string MedicamentDose { get; set; } = null!;

    public float MedicamentPrice { get; set; }

    public virtual ICollection<Reception> Receptions { get; set; } = new List<Reception>();
}
