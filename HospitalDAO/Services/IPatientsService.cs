using HospitalDAO.Models;

namespace HospitalDAO.Services
{
    public interface IPatientsService
    {
        public IEnumerable<Patient> GetPatients(int rowsNumber);

        public void AddPatients(string cacheKey, int rowsNumber);

        public IEnumerable<Patient> GetPatients(string cacheKey, int rowsNumber);

        public int GetPatientsCount();
    }
}
