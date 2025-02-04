using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class CorrelativeNumberService : ICorrelativeNumberService
    {
        private readonly IGenericRepository<CorrelativeNumber> _repositoryCorrelativeNumber;
        public CorrelativeNumberService(IUnitOfWork unitOfWork)
        {
            _repositoryCorrelativeNumber = unitOfWork.Repository<CorrelativeNumber>();
        }

        public async Task<CorrelativeNumber> Get(int? idTienda, string management)
        {
            var queryRepo = await _repositoryCorrelativeNumber.Query();

            var query = queryRepo.Where(n => n.Management == management);

            if (idTienda != null)
            {
                query = query.Where(n => n.IdTienda == idTienda);
            }

            var correlative = await query.FirstOrDefaultAsync();

            if (correlative == null)
            {
                throw new Exception($"No se encontró el número correlativo para la gestión '{management}'.");
            }

            lock (correlative)
            {
                correlative.LastNumber += 1;
                correlative.DateUpdate = TimeHelper.GetArgentinaTime();
            }

            return correlative;
        }


        public async Task<string> GetSerialNumberAndSave(int? idTienda, string management)
        {
            var correlative = await Get(idTienda, management);

            await _repositoryCorrelativeNumber.Edit(correlative);

            string ceros = new string('0', correlative.QuantityDigits.Value);
            string saleNumber = (ceros + correlative.LastNumber.ToString())
                                 .Substring(ceros.Length + correlative.LastNumber.ToString().Length - correlative.QuantityDigits.Value);

            return saleNumber;
        }

        public async Task<string> GetSerialNumber(int? idTienda, string management)
        {
            var correlative = await Get(idTienda, management);

            string ceros = new string('0', correlative.QuantityDigits.Value);
            string saleNumber = (ceros + correlative.LastNumber.ToString())
                                 .Substring(ceros.Length + correlative.LastNumber.ToString().Length - correlative.QuantityDigits.Value);

            return saleNumber;
        }

        public async Task EditLastNumber(int? idTienda, string management, int lastNumber)
        {
            var correlative = await Get(idTienda, management);
            correlative.LastNumber = lastNumber;

            await _repositoryCorrelativeNumber.Edit(correlative);
        }

        public async Task<CorrelativeNumber> CreateSerialNumberSale(int idTienda)
        {
            var c = new CorrelativeNumber()
            {
                LastNumber = 0,
                IdTienda = idTienda,
                QuantityDigits = 6,
                Management = "Sale",
                DateUpdate = TimeHelper.GetArgentinaTime()
            };

            await _repositoryCorrelativeNumber.Add(c);

            return c;
        }
    }
}
