using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class ReceptionsService(HospitalContext dbContext, IMemoryCache memoryCache) : IReceptionsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка рецептов из базы
        public IEnumerable<Reception> GetReceptions(int rowsNumber)
        {
            return _dbContext.Receptions.Take(rowsNumber).ToList();
        }

        // Добавление списка рецептов в кэш
        public void AddReceptions(string cacheKey, int rowsNumber)
        {
            IEnumerable<Reception> receptions = _dbContext.Receptions.Take(rowsNumber).ToList();
            if (receptions != null)
            {
                _memoryCache.Set(cacheKey, receptions, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка рецептов из кэша или из базы, если нет в кэше
        public IEnumerable<Reception> GetReceptions(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Reception> receptions))
            {
                receptions = _dbContext.Receptions.Take(rowsNumber).ToList();
                if (receptions != null)
                {
                    _memoryCache.Set(cacheKey, receptions,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return receptions.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetReceptionsCount()
        {
            return _dbContext.Receptions.Count();
        }
    }
}
