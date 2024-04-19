using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using PointOfSale.Model.Output;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class IvaService : IIvaService
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        private readonly IGenericRepository<Sale> _saleRepo;
        private readonly IGenericRepository<Gastos> _gastosRepo;
        private readonly IGenericRepository<ProveedorMovimiento> _movRepo;

        public IvaService(IGenericRepository<Sale> saleRepo, IGenericRepository<Gastos> gastosRepo, IGenericRepository<ProveedorMovimiento> movRepo)
        {
            _saleRepo = saleRepo;
            _gastosRepo = gastosRepo;
            _movRepo = movRepo;
        }

        public List<DatesFilterIvaReportOutput> GetDatesFilterList(int idTienda)
        {
            var meses = new List<DatesFilterIvaReportOutput>();

            var fechasMin = new List<DateTime>();

            try
            {

                var querySale = $"select top 1 * from sale where idTienda= {idTienda} order by idSale";
                var firstSale = _saleRepo.SqlRaw(querySale).FirstOrDefault();

                if (firstSale != null)
                {
                    if (firstSale.RegistrationDate.HasValue)
                        fechasMin.Add(firstSale.RegistrationDate.Value);
                }

                var queryGasto = $"select top 1 * from gastos where idTienda= {idTienda} order by idGastos";
                var firstGasto = _gastosRepo.SqlRaw(queryGasto).FirstOrDefault();

                if (firstGasto != null)
                {
                    fechasMin.Add(firstGasto.RegistrationDate);
                }

                var queryProv = $"select top 1 * from ProveedorMovimiento where idTienda= {idTienda} order by idProveedorMovimiento";
                var firstProv = _movRepo.SqlRaw(queryProv).FirstOrDefault();

                if (firstProv != null)
                {
                    fechasMin.Add(firstProv.RegistrationDate);
                }
            }
            catch (Exception e)
            {

                throw;
            }

            var fechaInicio = fechasMin.Min().Date;
            fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);

            CultureInfo cultura = CultureInfo.CreateSpecificCulture("es-ES");

            DateTime fechaTemp = DateTimeNowArg;
            fechaTemp = new DateTime(fechaTemp.Year, fechaTemp.Month, 1);

            while (fechaTemp >= fechaInicio)
            {
                string id = fechaTemp.ToString("MM-yyyy");
                string text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fechaTemp.ToString("MMMM-yyyy", cultura));
                meses.Add(new DatesFilterIvaReportOutput(id, text));
                fechaTemp = fechaTemp.AddMonths(-1);
            }

            return meses;
        }

        public async Task<List<Gastos>> GetGastosImports(int idTienda, DateTime start_date, DateTime end_date)
        {
            var query = await _gastosRepo.Query();
            return query.Where(_ =>
                _.IdTienda == idTienda &&
                _.Iva > 0 &&
                _.RegistrationDate.Date >= start_date.Date &&
                _.RegistrationDate.Date <= end_date.Date &&
                _.EstadoPago == EstadoPago.Pagado)
                .Include(_=>_.TipoDeGasto)
                .OrderByDescending(_ => _.RegistrationDate)
                .ToList();
        }

        public async Task<List<Sale>> GetSaleImports(int idTienda, DateTime start_date, DateTime end_date)
        {
            var query = await _saleRepo.Query();
            return query.Where(v =>
                    v.IdTienda == idTienda &&
                    v.RegistrationDate.Value.Date >= start_date.Date &&
                    v.RegistrationDate.Value.Date <= end_date.Date&&
                    v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu)
                .Include(_ => _.TypeDocumentSaleNavigation)
                .Include(dv => dv.DetailSales)
                .OrderByDescending(_ => _.RegistrationDate)
                .ToList();
        }

        public async Task<List<ProveedorMovimiento>> GetMovProveedoresImports(int idTienda, DateTime start_date, DateTime end_date)
        {
            var query = await _movRepo.Query();
            return query.Where(_ =>
                _.idTienda == idTienda &&
                _.Iva > 0 &&
                _.RegistrationDate.Date >= start_date.Date &&
                _.RegistrationDate.Date <= end_date.Date &&
                _.EstadoPago == EstadoPago.Pagado)
                .Include(_ => _.Proveedor)
                .OrderByDescending(_ => _.RegistrationDate)
                .ToList();
        }
    }
}
