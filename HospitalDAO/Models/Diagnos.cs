using System;
using System.Collections.Generic;

namespace HospitalDAO.Models;

public partial class Diagnos
{
    public int DiagnosId { get; set; }

    public string DiagnosName { get; set; } = null!;

    public string? DiagnosDescription { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
