using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class SpecializationsService(HospitalContext dbContext, IMemoryCache memoryCache) : ISpecializationsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка специализаций из базы
        public IEnumerable<Specialization> GetSpecializations(int rowsNumber)
        {
            return _dbContext.Specializations.Take(rowsNumber).ToList();
        }

        // Добавление списка специализаций в кэш
        public void AddSpecializations(string cacheKey, int rowsNumber)
        {
            IEnumerable<Specialization> specializations = _dbContext.Specializations.Take(rowsNumber).ToList();
            if (specializations != null)
            {
                _memoryCache.Set(cacheKey, specializations, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка специализаций из кэша или из базы, если нет в кэше
        public IEnumerable<Specialization> GetSpecializations(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Specialization> specializations))
            {
                specializations = _dbContext.Specializations.Take(rowsNumber).ToList();
                if (specializations != null)
                {
                    _memoryCache.Set(cacheKey, specializations,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return specializations.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetSpecializationsCount()
        {
            return _dbContext.Specializations.Count();
        }
    }
}
