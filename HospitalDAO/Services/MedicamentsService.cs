using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class MedicamentsService(HospitalContext dbContext, IMemoryCache memoryCache) : IMedicamentsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка лекарств из базы
        public IEnumerable<Medicament> GetMedicaments(int rowsNumber)
        {
            return _dbContext.Medicaments.Take(rowsNumber).ToList();
        }

        // Добавление списка лекарств в кэш
        public void AddMedicaments(string cacheKey, int rowsNumber)
        {
            IEnumerable<Medicament> medicaments = _dbContext.Medicaments.Take(rowsNumber).ToList();
            if (medicaments != null)
            {
                _memoryCache.Set(cacheKey, medicaments, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка лекарств из кэша или из базы, если нет в кэше
        public IEnumerable<Medicament> GetMedicaments(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Medicament> medicaments))
            {
                medicaments = _dbContext.Medicaments.Take(rowsNumber).ToList();
                if (medicaments != null)
                {
                    _memoryCache.Set(cacheKey, medicaments,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return medicaments.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetMedicamentsCount()
        {
            return _dbContext.Medicaments.Count();
        }
    }
}
