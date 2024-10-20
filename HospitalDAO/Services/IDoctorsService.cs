using HospitalDAO.Models;

namespace HospitalDAO.Services
{
    public interface IDoctorsService
    {
        public IEnumerable<Doctor> GetDoctors(int rowsNumber);

        public void AddDoctors(string cacheKey, int rowsNumber);

        public IEnumerable<Doctor> GetDoctors(string cacheKey, int rowsNumber);

        public int GetDoctorsCount();
    }
}
