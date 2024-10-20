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
    public class DiagnosisService(HospitalContext dbContext, IMemoryCache memoryCache) : IDiagnosisService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка емкостей из базы
        public IEnumerable<Diagnos> GetDiagnosis(int rowsNumber)
        {
            return _dbContext.Diagnosis.Take(rowsNumber).ToList();
        }

        // Добавление списка емкостей в кэш
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

        // Получение списка емкостей из кэша или из базы, если нет в кэше
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
