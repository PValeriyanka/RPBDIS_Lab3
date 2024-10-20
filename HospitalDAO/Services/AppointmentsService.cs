using HospitalDAO.Data;
using HospitalDAO.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HospitalDAO.Services
{
    public class AppointmentsService(HospitalContext dbContext, IMemoryCache memoryCache) : IAppointmentsService
    {
        private readonly HospitalContext _dbContext = dbContext;
        private readonly IMemoryCache _memoryCache = memoryCache;

        // Получение списка приемов из базы
        public IEnumerable<Appointment> GetAppointments(int rowsNumber)
        {
            return _dbContext.Appointments.Take(rowsNumber).ToList();
        }

        // Добавление списка приемов в кэш
        public void AddAppointments(string cacheKey, int rowsNumber)
        {
            IEnumerable<Appointment> appointments = _dbContext.Appointments.Take(rowsNumber).ToList();
            if (appointments != null)
            {
                _memoryCache.Set(cacheKey, appointments, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(286)
                });
            }
        }

        // Получение списка приемов из кэша или из базы, если нет в кэше
        public IEnumerable<Appointment> GetAppointments(string cacheKey, int rowsNumber)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Appointment> appointments))
            {
                appointments = _dbContext.Appointments.Take(rowsNumber).ToList();
                if (appointments != null)
                {
                    _memoryCache.Set(cacheKey, appointments,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(286)));
                }
            }
            return appointments.Take(rowsNumber).ToList();
        }

        // Получение количества записей
        public int GetAppointmentsCount()
        {
            return _dbContext.Appointments.Count();
        }
    }
}
