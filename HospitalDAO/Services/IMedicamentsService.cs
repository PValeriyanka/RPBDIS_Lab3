using HospitalDAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalDAO.Services
{
    public interface IMedicamentsService
    {
        public IEnumerable<Medicament> GetMedicaments(int rowsNumber);

        public void AddMedicaments(string cacheKey, int rowsNumber);

        public IEnumerable<Medicament> GetMedicaments(string cacheKey, int rowsNumber);

        public int GetMedicamentsCount();
    }
}
