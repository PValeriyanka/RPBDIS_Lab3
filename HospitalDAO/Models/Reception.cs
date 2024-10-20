namespace HospitalDAO.Models;

public partial class Reception
{
    public int ReceptionId { get; set; }

    public int AppointmentId { get; set; }

    public int MedicamentId { get; set; }

    public string? ReceptionDose { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Medicament Medicament { get; set; } = null!;
}
