using HospitalDAO.Models;

namespace HospitalDAO.Services
{
    public interface IReceptionsService
    {
        public IEnumerable<Reception> GetReceptions(int rowsNumber);

        public void AddReceptions(string cacheKey, int rowsNumber);

        public IEnumerable<Reception> GetReceptions(string cacheKey, int rowsNumber);

        public int GetReceptionsCount();
    }
}
