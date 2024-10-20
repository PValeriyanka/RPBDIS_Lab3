using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HospitalDAO.Models;

namespace HospitalDAO.Data
{
    public class HospitalContext : DbContext
    {
        public HospitalContext(DbContextOptions<HospitalContext> options) : base(options)
        {
        }
    
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Diagnos> Diagnosis { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Medicament> Medicaments { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Reception> Receptions { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
    }
}
