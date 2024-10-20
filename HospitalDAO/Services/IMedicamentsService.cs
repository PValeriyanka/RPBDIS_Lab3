using HospitalDAO.Models;

namespace HospitalDAO.Services
{
    public interface IMedicamentsService
    {
        public IEnumerable<Medicament> GetMedicaments(int rowsNumber);

        public void AddMedicaments(string cacheKey, int rowsNumber);

        public IEnumerable<Medicament> GetMedicaments(string cacheKey, int rowsNumber);

        public int GetMedicamentsCount();
    }
}
