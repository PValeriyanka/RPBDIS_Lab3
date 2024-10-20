namespace HospitalDAO.Models;

public partial class Specialization : IComparable<Specialization>
{
    public int SpecializationId { get; set; }

    public string SpecializationName { get; set; } = null!;

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

    public int CompareTo(Specialization other)
    {
        if (other == null) return 1;
        return this.SpecializationName.CompareTo(other.SpecializationName);
    }
}
