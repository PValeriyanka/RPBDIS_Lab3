namespace HospitalDAO.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int DoctorId { get; set; }

    public int PatientId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public int DiagnosId { get; set; }

    public virtual Diagnos Diagnos { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Reception> Receptions { get; set; } = new List<Reception>();
}
