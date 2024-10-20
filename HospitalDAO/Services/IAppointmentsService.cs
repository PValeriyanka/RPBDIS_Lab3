using HospitalDAO.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
