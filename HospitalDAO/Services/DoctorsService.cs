﻿using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class DoctorsService(HospitalContext dbContext, IMemoryCache memoryCache) : IDoctorsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка врачей из базы
        public IEnumerable<Doctor> GetDoctors(int rowsNumber)
        {
            return _dbContext.Doctors.Take(rowsNumber).ToList();
        }

        // Добавление списка врачей в кэш
        public void AddDoctors(string cacheKey, int rowsNumber)
        {
            IEnumerable<Doctor> doctors = _dbContext.Doctors.Take(rowsNumber).ToList();
            if (doctors != null)
            {
                _memoryCache.Set(cacheKey, doctors, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка врачей из кэша или из базы, если нет в кэше
        public IEnumerable<Doctor> GetDoctors(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Doctor> doctors))
            {
                doctors = _dbContext.Doctors.Take(rowsNumber).ToList();
                if (doctors != null)
                {
                    _memoryCache.Set(cacheKey, doctors,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return doctors.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetDoctorsCount()
        {
            return _dbContext.Doctors.Count();
        }
    }
}
