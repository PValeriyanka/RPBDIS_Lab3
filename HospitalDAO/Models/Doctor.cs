using System;
using System.Collections.Generic;

namespace HospitalDAO.Models;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string DoctorLastName { get; set; } = null!;

    public string DoctorFirstName { get; set; } = null!;

    public string? DoctorSurname { get; set; }

    public int SpecializationId { get; set; }

    public string? ContactData { get; set; }

    public string Password { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Specialization Specialization { get; set; } = null!;
}
