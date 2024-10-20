using HospitalDAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalDAO.Services
{
    public interface IDiagnosisService
    {
        public IEnumerable<Diagnos> GetDiagnosis(int rowsNumber);

        public void AddDiagnosis(string cacheKey, int rowsNumber);

        public IEnumerable<Diagnos> GetDiagnosis(string cacheKey, int rowsNumber);

        public int GetDiagnosisCount();
    }
}
