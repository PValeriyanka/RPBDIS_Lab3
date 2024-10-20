using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalDAO.Models;
using HospitalDAO.Data;

namespace HospitalDAO.Services
{
    public class ReceptionsService(HospitalContext dbContext, IMemoryCache memoryCache) : IReceptionsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка емкостей из базы
        public IEnumerable<Reception> GetReceptions(int rowsNumber)
        {
            return _dbContext.Receptions.Take(rowsNumber).ToList();
        }

        // Добавление списка емкостей в кэш
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

        // Получение списка емкостей из кэша или из базы, если нет в кэше
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
