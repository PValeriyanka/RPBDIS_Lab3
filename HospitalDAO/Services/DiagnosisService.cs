using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class DiagnosisService(HospitalContext dbContext, IMemoryCache memoryCache) : IDiagnosisService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка диагнозов из базы
        public IEnumerable<Diagnos> GetDiagnosis(int rowsNumber)
        {
            return _dbContext.Diagnosis.Take(rowsNumber).ToList();
        }

        // Добавление списка диагнозов в кэш
        public void AddDiagnosis(string cacheKey, int rowsNumber)
        {
            IEnumerable<Diagnos> diagnosis = _dbContext.Diagnosis.Take(rowsNumber).ToList();
            if (diagnosis != null)
            {
                _memoryCache.Set(cacheKey, diagnosis, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка диагнозов из кэша или из базы, если нет в кэше
        public IEnumerable<Diagnos> GetDiagnosis(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Diagnos> diagnosis))
            {
                diagnosis = _dbContext.Diagnosis.Take(rowsNumber).ToList();
                if (diagnosis != null)
                {
                    _memoryCache.Set(cacheKey, diagnosis,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return diagnosis.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetDiagnosisCount()
        {
            return _dbContext.Diagnosis.Count();
        }
    }
}
