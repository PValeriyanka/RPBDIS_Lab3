using System;
using System.Collections.Generic;

namespace HospitalDAO.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string PatientLastName { get; set; } = null!;

    public string PatientFirstName { get; set; } = null!;

    public string? PatientSurname { get; set; }

    public DateOnly BirthDate { get; set; }

    public string? ContactData { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
