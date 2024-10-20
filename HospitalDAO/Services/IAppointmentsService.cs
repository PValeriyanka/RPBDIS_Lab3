using HospitalDAO.Models;

namespace HospitalDAO.Services
{
    public interface IAppointmentsService
    {
        public IEnumerable<Appointment> GetAppointments(int rowsNumber);

        public void AddAppointments(string cacheKey, int rowsNumber);

        public IEnumerable<Appointment> GetAppointments(string cacheKey, int rowsNumber);

        public int GetAppointmentsCount();
    }
}
