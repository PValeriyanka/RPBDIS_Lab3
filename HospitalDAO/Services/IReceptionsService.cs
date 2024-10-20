using HospitalDAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalDAO.Services
{
    public interface IReceptionsService
    {
        public IEnumerable<Reception> GetReceptions(int rowsNumber);

        public void AddReceptions(string cacheKey, int rowsNumber);

        public IEnumerable<Reception> GetReceptions(string cacheKey, int rowsNumber);

        public int GetReceptionsCount();
    }
}
