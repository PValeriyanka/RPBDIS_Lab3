namespace HospitalDAO.Models;

public partial class Medicament : IComparable<Medicament>
{
    public int MedicamentId { get; set; }

    public string MedicamentName { get; set; } = null!;

    public string MedicamentDose { get; set; } = null!;

    public float MedicamentPrice { get; set; }

    public virtual ICollection<Reception> Receptions { get; set; } = new List<Reception>();

    public int CompareTo(Medicament other)
    {
        if (other == null) return 1;
        return this.MedicamentName.CompareTo(other.MedicamentName);

    }
}
