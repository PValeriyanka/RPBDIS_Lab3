using HospitalDAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalDAO.Services
{
    public interface ISpecializationsService
    {
        public IEnumerable<Specialization> GetSpecializations(int rowsNumber);
        
        public void AddSpecializations(string cacheKey, int rowsNumber);

        public IEnumerable<Specialization> GetSpecializations(string cacheKey, int rowsNumber);

        public int GetSpecializationsCount();
    }
}
