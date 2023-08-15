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
        private readonly IGenericRepository<Category> _repositoryCategory;
        private readonly IGenericRepository<Product> _repositoryProduct;
        private DateTime StartDate = DateTime.Now;

        public DashBoardService(
            ISaleRepository repositorySale,
            IGenericRepository<DetailSale> repositoryDetailSale,
            IGenericRepository<Category> repositoryCategory,
            IGenericRepository<Product> repositoryProduct
            )
        {

            _repositorySale = repositorySale;
            _repositoryDetailSale = repositoryDetailSale;
            _repositoryCategory = repositoryCategory;
            _repositoryProduct = repositoryProduct;

            //StartDate = StartDate.AddDays(-7);

        }
        public async Task<int> TotalSalesLastWeek()
        {
            try
            {
                IQueryable<Sale> query = await _repositorySale.Query(v => v.RegistrationDate.Value.Date >= StartDate.Date);
                int total = query.Count();
                return total;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> TotalIncomeLastWeek()
        {
            try
            {
                IQueryable<Sale> query = await _repositorySale.Query(v => v.RegistrationDate.Value.Date >= StartDate.Date);

                decimal resultado = query
                    .Select(v => v.Total)
                    .Sum(v => v.Value);

                return Convert.ToString(resultado, new CultureInfo("es-PE"));
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> TotalProducts()
        {
            try
            {
                IQueryable<Product> query = await _repositoryProduct.Query();
                int total = query.Count();
                return total;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> TotalCategories()
        {
            try
            {
                IQueryable<Category> query = await _repositoryCategory.Query();
                int total = query.Count();
                return total;
            }
            catch
            {
                throw;
            }
        }
        public async Task<GraficoVentasConComparacion> GetSales(TypeValuesDashboard typeValues)
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
                        resultados.VentasActualesHour = await GetSalesHour(start);
                        resultados.VentasComparacionHour = await GetComparationHour(start, dateCompare);
                        break;

                    case TypeValuesDashboard.Semana:
                        start = start.AddDays((-(int)DateTime.Now.Date.DayOfWeek) + 1);
                        dateCompare = start.AddDays(-7);
                        resultados.VentasActuales = await GetSalesActuales(start);
                        resultados.VentasComparacion = await GetComparation(start, dateCompare);

                        break;

                    case TypeValuesDashboard.Mes:
                        start = start.AddDays((-DateTime.Now.Date.Day) + 1);
                        dateCompare = start.AddMonths(-1);
                        resultados.VentasActuales = await GetSalesActuales(start);
                        resultados.VentasComparacion = await GetComparation(start, dateCompare);

                        break;
                }


                return resultados;

            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVenta(TypeValuesDashboard typeValues)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.IdTypeDocumentSaleNavigation)
                .Where(vd => vd.RegistrationDate.Value.Date >= start.Date)
                .GroupBy(v => v.IdTypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_=>_.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_=>_.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        private async Task<Dictionary<DateTime, decimal>> GetSalesActuales(DateTime start)
        {
            IQueryable<Sale> queryVentasActuales = await _repositorySale.Query(v => v.RegistrationDate.Value.Date >= start.Date);
            return queryVentasActuales
                .GroupBy(v => v.RegistrationDate.Value.Date).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        private async Task<Dictionary<int, decimal>> GetSalesHour(DateTime start)
        {
            var resultados = new GraficoVentasConComparacion();

            IQueryable<Sale> queryVentasActuales = await _repositorySale.Query(v => v.RegistrationDate.Value.Date >= start.Date);
            return queryVentasActuales.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        private async Task<Dictionary<DateTime, decimal>> GetComparation(DateTime start, DateTime dateCompare)
        {
            var queryVentasComparacion = await _repositorySale.Query(v => v.RegistrationDate.Value.Date < start.Date && v.RegistrationDate.Value.Date >= dateCompare.Date);
            return queryVentasComparacion
                .GroupBy(_ => _.RegistrationDate.Value.Date).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        private async Task<Dictionary<int, decimal>> GetComparationHour(DateTime start, DateTime dateCompare)
        {
            var resultados = new GraficoVentasConComparacion();

            var queryVentasComparacionHour = await _repositorySale.Query(v => v.RegistrationDate.Value.Date < start.Date && v.RegistrationDate.Value.Date >= dateCompare.Date);
            return queryVentasComparacionHour.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        public async Task<Dictionary<string, int>> ProductsTop(TypeValuesDashboard typeValues)
        {
            DateTime dateCompare, start;
            FechasParaQuery(typeValues, out dateCompare, out start);

            try
            {
                IQueryable<DetailSale> query = await _repositoryDetailSale.Query();

                Dictionary<string, int> resultado = query
                    .Include(v => v.IdSaleNavigation)
                    .Where(dv => dv.IdSaleNavigation.RegistrationDate.Value.Date < start && dv.IdSaleNavigation.RegistrationDate.Value.Date > dateCompare.Date)
                    .GroupBy(dv => dv.DescriptionProduct).OrderByDescending(g => g.Count())
                    .Select(dv => new { product = dv.Key, total = dv.Count() }).Take(10)
                    .ToDictionary(keySelector: r => r.product, elementSelector: r => r.total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }

        private static void FechasParaQuery(TypeValuesDashboard typeValues, out DateTime dateCompare, out DateTime start)
        {
            dateCompare = DateTime.Now;
            start = DateTime.Now;
            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:
                    dateCompare = start.AddDays(-1);
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
    }
}
