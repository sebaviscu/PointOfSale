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

        public DashBoardService(
            ISaleRepository repositorySale,
            IGenericRepository<DetailSale> repositoryDetailSale, 
            IGenericRepository<ProveedorMovimiento> proveedorMovimiento
            )
        {

            _repositorySale = repositorySale;
            _repositoryDetailSale = repositoryDetailSale;
            _proveedorMovimiento = proveedorMovimiento;
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
                        start = start.AddDays((-(int)DateTime.Now.Date.DayOfWeek) + 1);
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
        public async Task<List<ProveedorMovimiento>> GetMovimientosProveedoresByTienda(TypeValuesDashboard typeValues, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<ProveedorMovimiento> query = await _proveedorMovimiento.Query(_=> _.RegistrationDate.Date >= start.Date && _.idTienda == idTienda);

            return query.ToList();
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVenta(TypeValuesDashboard typeValues, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.IdTypeDocumentSaleNavigation)
                .Where(vd => vd.RegistrationDate.Value.Date >= start.Date && vd.IdClienteMovimiento == null && vd.IdTienda == idTienda)
                .GroupBy(v => v.IdTypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }


        private async Task<(Dictionary<DateTime, decimal> Ventas, int CantidadVentas)> GetSalesActuales(DateTime start, int idTienda)
        {
            IQueryable<Sale> queryVentasActuales = await _repositorySale.Query(v => 
                            v.RegistrationDate.Value.Date >= start.Date 
                            && v.IdClienteMovimiento == null
                            && v.IdTienda == idTienda);

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

            IQueryable<Sale> queryVentasActuales = await _repositorySale.Query(v => 
                            v.RegistrationDate.Value.Date >= start.Date 
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
            var queryVentasComparacion = await _repositorySale.Query(v => 
                            v.RegistrationDate.Value.Date < start.Date 
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

            var queryVentasComparacionHour = await _repositorySale.Query(v => 
                            v.RegistrationDate.Value.Date < start.Date 
                            && v.RegistrationDate.Value.Date >= dateCompare.Date 
                            && v.IdClienteMovimiento == null
                            && v.IdTienda == idTienda);
            return queryVentasComparacionHour.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        public async Task<Dictionary<string, decimal?>> ProductsTopByCategory(TypeValuesDashboard typeValues, string category, int idTienda)
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
                            .Where(dv =>
                                    dv.IdSaleNavigation.RegistrationDate.Value.Date >= start
                                    && dv.IdSaleNavigation.IdTienda == idTienda);
                }
                else
                {
                    query = query
                            .Include(v => v.IdSaleNavigation)
                            .Where(dv => 
                                    dv.IdSaleNavigation.RegistrationDate.Value.Date >= start
                                    && dv.IdSaleNavigation.IdTienda == idTienda
                                    && dv.CategoryProducty == category);
                }

                Dictionary<string, decimal?> resultado = query
                .GroupBy(dv => dv.DescriptionProduct).OrderByDescending(g => g.Sum(_ => _.Quantity))
                .Select(dv => new { product = dv.Key, total = dv.Sum(_ => _.Quantity) }).Take(10)
                .ToDictionary(keySelector: r => r.product, elementSelector: r => r.total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }
        public async Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurno(TypeValuesDashboard typeValues, int turno, int idTienda)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.IdTypeDocumentSaleNavigation)
                .Where(vd => vd.RegistrationDate.Value.Date >= start.Date
                        && vd.IdClienteMovimiento == null
                        && vd.IdTurno == turno
                        && vd.IdTienda == idTienda)
                .GroupBy(v => v.IdTypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
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
                    start = start.AddDays((-(int)DateTime.Now.Date.DayOfWeek) + 1);
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
