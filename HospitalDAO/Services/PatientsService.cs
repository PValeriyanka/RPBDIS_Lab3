using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class PatientsService(HospitalContext dbContext, IMemoryCache memoryCache) : IPatientsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка пациентов из базы
        public IEnumerable<Patient> GetPatients(int rowsNumber)
        {
            return _dbContext.Patients.Take(rowsNumber).ToList();
        }

        // Добавление списка пациентов в кэш
        public void AddPatients(string cacheKey, int rowsNumber)
        {
            IEnumerable<Patient> patients = _dbContext.Patients.Take(rowsNumber).ToList();
            if (patients != null)
            {
                _memoryCache.Set(cacheKey, patients, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка пациентов из кэша или из базы, если нет в кэше
        public IEnumerable<Patient> GetPatients(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Patient> patients))
            {
                patients = _dbContext.Patients.Take(rowsNumber).ToList();
                if (patients != null)
                {
                    _memoryCache.Set(cacheKey, patients,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return patients.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetPatientsCount()
        {
            return _dbContext.Patients.Count();
        }
    }
}
