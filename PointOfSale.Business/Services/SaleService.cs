using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IGenericRepository<Product> _repositoryProduct;
        private readonly IGenericRepository<Cliente> _repositoryCliente;
        private readonly ISaleRepository _repositorySale;
        public SaleService(IGenericRepository<Product> repositoryProduct, ISaleRepository repositorySale, IGenericRepository<Cliente> repositoryCliente)
        {
            _repositoryProduct = repositoryProduct;
            _repositorySale = repositorySale;
            _repositoryCliente = repositoryCliente;
        }

        public async Task<List<Product>> GetProducts(string search)
        {
            var list = new List<Product>();
            if (search.Contains(' '))
            {
                var split = search.Split(' ');
                var query = "select * from Product where ";
                for (int i = 0; i < split.Length; i++)
                {
                    query += $"description LIKE '%{split[i]}%' ";
                    if (i < split.Length - 1) query += " and ";
                }
                list = _repositoryProduct.SqlRaw(query).ToList();
            }
            else
            {
                IQueryable<Product> query = await _repositoryProduct.Query(p =>
                           p.IsActive == true &&
                           string.Concat(p.BarCode, p.Description).Contains(search));

                list = query.Include(c => c.IdCategoryNavigation).ToList();
            }

            return list;
        }

        public async Task<List<Cliente>> GetClients(string search)
        {
            IQueryable<Cliente> query = await _repositoryCliente.Query(p =>
           string.Concat(p.Cuil, p.Nombre).Contains(search));

            return query.ToList();
        }

        public async Task<Sale> Register(Sale entity)
        {
            try
            {
                return await _repositorySale.Register(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Sale>> SaleHistory(string SaleNumber, string StarDate, string EndDate)
        {
            IQueryable<Sale> query = await _repositorySale.Query();
            StarDate = StarDate is null ? "" : StarDate;
            EndDate = EndDate is null ? "" : EndDate;

            if (StarDate != "" && EndDate != "")
            {

                DateTime start_date = DateTime.ParseExact(StarDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime end_date = DateTime.ParseExact(EndDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

                return query.Where(v =>
                    v.RegistrationDate.Value.Date >= start_date.Date &&
                    v.RegistrationDate.Value.Date <= end_date.Date
                )
                .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .ToList();
            }
            else
            {
                return query.Where(v => v.SaleNumber == SaleNumber)
                .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .ToList();
            }
        }

        public async Task<Sale> Detail(string SaleNumber)
        {
            IQueryable<Sale> query = await _repositorySale.Query(v => v.SaleNumber == SaleNumber);

            return query
               .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
               .Include(u => u.IdUsersNavigation)
               .Include(dv => dv.DetailSales)
               .First();
        }

        public async Task<List<DetailSale>> Report(string StartDate, string EndDate)
        {
            DateTime start_date = DateTime.ParseExact(StartDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
            DateTime end_date = DateTime.ParseExact(EndDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

            List<DetailSale> lista = await _repositorySale.Report(start_date, end_date);

            return lista;
        }


        public async Task<Sale> Edit(Sale entity)
        {
            try
            {
                Sale sale_found = await _repositorySale.Get(c => c.IdSale == entity.IdSale);

                sale_found.IdClienteMovimiento = entity.IdClienteMovimiento;

                bool response = await _repositorySale.Edit(entity);

                if (!response)
                    throw new TaskCanceledException("Venta no se pudo cambiar.");

                return sale_found;
            }
            catch
            {
                throw;
            }
        }
    }
}
