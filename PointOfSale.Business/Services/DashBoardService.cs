using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class DashBoardService : IDashBoardService
    {
        private readonly ISaleRepository _repositorySale;
        private readonly IGenericRepository<DetailSale> _repositoryDetailSale;
        private readonly IGenericRepository<ProveedorMovimiento> _proveedorMovimiento;
        private readonly IGenericRepository<Gastos> _gastosRepository;

        public DashBoardService(
            ISaleRepository repositorySale,
            IGenericRepository<DetailSale> repositoryDetailSale,
            IGenericRepository<ProveedorMovimiento> proveedorMovimiento,
            IGenericRepository<Gastos> gastosRepository
            )
        {

            _repositorySale = repositorySale;
            _repositoryDetailSale = repositoryDetailSale;
            _proveedorMovimiento = proveedorMovimiento;
            _gastosRepository = gastosRepository;
        }

        public async Task<GraficoVentasConComparacion> GetSales(TypeValuesDashboard typeValues, int idTienda)
        {
            var start = DateTime.Now;
            var resultados = new GraficoVentasConComparacion();
            var dateCompare = DateTime.Now;

            try
            {
                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        dateCompare = start.AddDays(-1);
                        var resp = await GetSalesHour(start, idTienda);
                        resultados.VentasActualesHour = resp.Ventas;
                        resultados.CantidadClientes = resp.CantidadVentas;
                        resultados.VentasComparacionHour = await GetComparationHour(start, dateCompare, idTienda);
                        break;

                    case TypeValuesDashboard.Semana:
                        var numberWeek = (int)DateTime.Now.Date.DayOfWeek == 0 ? 7 : (int)DateTime.Now.Date.DayOfWeek;
                        start = start.AddDays(-numberWeek + 1);
                        dateCompare = start.AddDays(-7);
                        var resp2 = await GetSalesActuales(start, idTienda);
                        resultados.VentasActuales = resp2.Ventas;
                        resultados.CantidadClientes = resp2.CantidadVentas;
                        resultados.VentasComparacion = await GetComparation(start, dateCompare, idTienda);

                        break;

                    case TypeValuesDashboard.Mes:
                        start = start.AddDays((-DateTime.Now.Date.Day) + 1);
                        dateCompare = start.AddMonths(-1);
                        var resp3 = await GetSalesActuales(start, idTienda);
                        resultados.VentasActuales = resp3.Ventas;
                        resultados.CantidadClientes = resp3.CantidadVentas;
                        resultados.VentasComparacion = await GetComparation(start, dateCompare, idTienda);

                        break;
                }


                return resultados;

            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetMovimientosProveedoresByTienda(TypeValuesDashboard typeValues, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(_ => _.RegistrationDate.Date >= start.Date && _.idTienda == idTienda);

            IQueryable<ProveedorMovimiento> query2 = await _proveedorMovimiento.Query();

            Dictionary<string, decimal> resultado = query2
                        .Include(v => v.Proveedor)
                        .Where(_ => _.RegistrationDate.Date >= start.Date && _.idTienda == idTienda && _.EstadoPago.Value == EstadoPago.Pagado)
                        .GroupBy(v => v.Proveedor.Nombre).OrderByDescending(g => g.Sum(_ => _.Importe))
                        .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                        .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVenta(TypeValuesDashboard typeValues, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(
                vd => vd.RegistrationDate.Value.Date >= start.Date 
                && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                && vd.IdClienteMovimiento == null 
                && vd.IdTienda == idTienda)
                .GroupBy(v => v.TypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }


        private async Task<(Dictionary<DateTime, decimal> Ventas, int CantidadVentas)> GetSalesActuales(DateTime start, int idTienda)
        {
            var query = await _repositorySale.Query();

            var queryVentasActuales = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date >= start.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && v.IdClienteMovimiento == null
                            && v.IdTienda == idTienda);

            //IQueryable<Sale> queryVentasActuales = await _repositorySale.Query(v =>
            //                v.RegistrationDate.Value.Date >= start.Date
            //                && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
            //                && v.IdClienteMovimiento == null
            //                && v.IdTienda == idTienda);

            var cantVentas = queryVentasActuales.Count();


            var resp = queryVentasActuales
                .GroupBy(v => v.RegistrationDate.Value.Date).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);

            return (resp, cantVentas);
        }

        private async Task<(Dictionary<int, decimal> Ventas, int CantidadVentas)> GetSalesHour(DateTime start, int idTienda)
        {
            var resultados = new GraficoVentasConComparacion();

            var query = await _repositorySale.Query();

            var queryVentasActuales = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date >= start.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && v.IdClienteMovimiento == null
                            && v.IdTienda == idTienda);

            var cantVentas = queryVentasActuales.Count();

            var resp = queryVentasActuales.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);

            return (resp, cantVentas);
        }

        private async Task<Dictionary<DateTime, decimal>> GetComparation(DateTime start, DateTime dateCompare, int idTienda)
        {
            var query = await _repositorySale.Query();

            var queryVentasComparacion = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date < start.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && v.RegistrationDate.Value.Date >= dateCompare.Date
                            && v.IdClienteMovimiento == null
                            && v.IdTienda == idTienda);

            return queryVentasComparacion
                .GroupBy(_ => _.RegistrationDate.Value.Date).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        private async Task<Dictionary<int, decimal>> GetComparationHour(DateTime start, DateTime dateCompare, int idTienda)
        {
            var resultados = new GraficoVentasConComparacion();

            var query = await _repositorySale.Query();

            var queryVentasComparacionHour = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date < start.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && v.RegistrationDate.Value.Date >= dateCompare.Date
                            && v.IdClienteMovimiento == null
                            && v.IdTienda == idTienda);

            return queryVentasComparacionHour.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        public async Task<Dictionary<string, string?>> ProductsTopByCategory(TypeValuesDashboard typeValues, string category, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);
            try
            {
                IQueryable<DetailSale> query = await _repositoryDetailSale.Query();

                if (category == "Todo")
                {
                    query = query
                            .Include(v => v.IdSaleNavigation)
                            .Include(v => v.IdSaleNavigation.TypeDocumentSaleNavigation)
                            .Include(v => v.Producto)
                            .Where(dv =>
                                    dv.IdSaleNavigation.RegistrationDate.Value.Date >= start.Date
                                    && dv.IdSaleNavigation.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                                    && dv.IdSaleNavigation.IdTienda == idTienda);
                }
                else
                {
                    query = query
                            .Include(v => v.IdSaleNavigation)
                            .Include(v => v.IdSaleNavigation.TypeDocumentSaleNavigation)
                            .Include(v => v.Producto)
                            .Where(dv =>
                                    dv.IdSaleNavigation.RegistrationDate.Value.Date >= start.Date
                                    && dv.IdSaleNavigation.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                                    && dv.IdSaleNavigation.IdTienda == idTienda
                                    && dv.CategoryProducty == category);
                }

                Dictionary<string, string?> resultado = query
                .GroupBy(dv => dv.DescriptionProduct).OrderByDescending(g => g.Sum(_ => _.Quantity))
                .Select(dv => new { product = dv.Key, total = dv.Sum(_ => _.Quantity) }).Take(10)
                .ToDictionary(keySelector: r => r.product, elementSelector: r => Math.Truncate(r.total.Value).ToString());

                return resultado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard typeValues, int turno, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(vd => vd.RegistrationDate.Value.Date >= start.Date
                        && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                        && vd.IdClienteMovimiento == null
                        && vd.IdTurno == turno
                        && vd.IdTienda == idTienda)
                .GroupBy(v => v.TypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }


        public async Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurno(TypeValuesDashboard typeValues, int turno, int idTienda)
        {
            DateTime dateCompare, start;

            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(vd => vd.IdClienteMovimiento == null
                        && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                        && vd.IdTurno == turno
                        && vd.IdTienda == idTienda)
                .GroupBy(v => v.TypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetGastos(TypeValuesDashboard typeValues, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Gastos> query = await _gastosRepository.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.TipoDeGasto)
                .Where(vd => vd.RegistrationDate.Date >= start.Date
                        && vd.IdTienda == idTienda
                        && vd.TipoDeGasto.GastoParticular != TipoDeGastoEnum.Sueldos
                        && vd.EstadoPago == EstadoPago.Pagado)
                .GroupBy(v => v.TipoDeGasto.Descripcion).OrderByDescending(g => g.Sum(_ => _.Importe))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }
        public async Task<Dictionary<string, decimal>> GetGastosSueldos(TypeValuesDashboard typeValues, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Gastos> query = await _gastosRepository.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.TipoDeGasto)
                .Where(vd => vd.RegistrationDate.Date >= start.Date
                        && vd.IdTienda == idTienda
                        && vd.TipoDeGasto.GastoParticular == TipoDeGastoEnum.Sueldos
                        && vd.EstadoPago == EstadoPago.Pagado)
                .GroupBy(v => v.TipoDeGasto.Descripcion).OrderByDescending(g => g.Sum(_ => _.Importe))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        private static void FechasParaQuery(TypeValuesDashboard typeValues, out DateTime dateCompare, out DateTime start)
        {
            dateCompare = DateTime.Now;
            start = DateTime.Now;
            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:
                    break;

                case TypeValuesDashboard.Semana:
                    var numberWeek = (int)DateTime.Now.Date.DayOfWeek == 0 ? 7 : (int)DateTime.Now.Date.DayOfWeek;
                    start = start.AddDays(-numberWeek + 1);
                    dateCompare = start.AddDays(-7);

                    break;

                case TypeValuesDashboard.Mes:
                    start = start.AddDays((-DateTime.Now.Date.Day) + 1);
                    dateCompare = start.AddMonths(-1);

                    break;
            }
        }
    }

    public class GraficoVentasConComparacion
    {
        public Dictionary<DateTime, decimal> VentasActuales { get; set; }
        public Dictionary<DateTime, decimal> VentasComparacion { get; set; }
        public Dictionary<int, decimal> VentasActualesHour { get; set; }
        public Dictionary<int, decimal> VentasComparacionHour { get; set; }
        public Dictionary<string, decimal> VentasPorTipoVenta { get; set; }

        public int CantidadClientes { get; set; }
    }
}
