namespace HospitalDAO.Models;

public partial class Diagnos : IComparable<Diagnos>
{
    public int DiagnosId { get; set; }

    public string DiagnosName { get; set; } = null!;

    public string? DiagnosDescription { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public int CompareTo(Diagnos other)
    {
        if (other == null) return 1;
        return this.DiagnosName.CompareTo(other.DiagnosName);
    }
}
