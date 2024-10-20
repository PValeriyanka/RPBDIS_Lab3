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
    public class MedicamentsService(HospitalContext dbContext, IMemoryCache memoryCache) : IMedicamentsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка емкостей из базы
        public IEnumerable<Medicament> GetMedicaments(int rowsNumber)
        {
            return _dbContext.Medicaments.Take(rowsNumber).ToList();
        }

        // Добавление списка емкостей в кэш
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

        // Получение списка емкостей из кэша или из базы, если нет в кэше
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
