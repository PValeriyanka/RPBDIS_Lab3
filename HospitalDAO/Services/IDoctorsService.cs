using HospitalDAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalDAO.Services
{
    public interface IDoctorsService
    {
        public IEnumerable<Doctor> GetDoctors(int rowsNumber);

        public void AddDoctors(string cacheKey, int rowsNumber);

        public IEnumerable<Doctor> GetDoctors(string cacheKey, int rowsNumber);

        public int GetDoctorsCount();
    }
}
