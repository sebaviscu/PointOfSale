using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Model;

namespace PointOfSale.Business.Contracts
{
    public interface ICorrelativeNumberService
    {
        Task<CorrelativeNumber> Get(int? idTienda, string management);
        Task<string> GetSerialNumberAndSave(int? idTienda, string management);
        Task<string> GetSerialNumber(int? idTienda, string management);
        Task EditLastNumber(int? idTienda, string management, int lastNumber);
        Task<CorrelativeNumber> CreateSerialNumberSale(int idTienda);
    }
}
