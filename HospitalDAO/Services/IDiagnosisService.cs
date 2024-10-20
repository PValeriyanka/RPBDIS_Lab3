using HospitalDAO.Models;

namespace HospitalDAO.Services
{
    public interface IDiagnosisService
    {
        public IEnumerable<Diagnos> GetDiagnosis(int rowsNumber);

        public void AddDiagnosis(string cacheKey, int rowsNumber);

        public IEnumerable<Diagnos> GetDiagnosis(string cacheKey, int rowsNumber);

        public int GetDiagnosisCount();
    }
}
