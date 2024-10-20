using HospitalDAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalDAO.Services
{
    public interface IPatientsService
    {
        public IEnumerable<Patient> GetPatients(int rowsNumber);

        public void AddPatients(string cacheKey, int rowsNumber);

        public IEnumerable<Patient> GetPatients(string cacheKey, int rowsNumber);

        public int GetPatientsCount();
    }
}
