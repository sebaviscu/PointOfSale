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
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IGenericRepository<Product> _repositoryProduct;
        private readonly IGenericRepository<Cliente> _repositoryCliente;
        private readonly ISaleRepository _repositorySale;
        private readonly ITypeDocumentSaleService _rTypeNumber;
        private readonly IProductService _rProduct;
        private readonly ITurnoService _turnoService;

        public SaleService(
            IGenericRepository<Product> repositoryProduct,
            ISaleRepository repositorySale,
            IGenericRepository<Cliente> repositoryCliente,
            ITypeDocumentSaleService rTypeNumber,
            IProductService rProduct,
            ITurnoService turnoService)
        {
            _repositoryProduct = repositoryProduct;
            _repositorySale = repositorySale;
            _repositoryCliente = repositoryCliente;
            _rTypeNumber = rTypeNumber;
            _rProduct = rProduct;
            _turnoService = turnoService;
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
                .OrderByDescending(_=>_.IdSale)
                .ToList();
            }
            else
            {
                return query.Where(v => v.SaleNumber == SaleNumber)
                .Include(tdv => tdv.IdTypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .OrderByDescending(_ => _.IdSale)
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

        public async Task<int> GenerarVentas(int idTienda, int idUser)
        {

            var tiposVentas = await _rTypeNumber.List();
            var productos = await _rProduct.List();
            var turno = await _turnoService.GetTurnoActual(idTienda);

            var random = new Random();
            var cantTipoVentas = tiposVentas.Count;
            var cantproductos = productos.Count;
            try
            {
                for (int i = 0; i < 10; i++) /*1200*/
                {
                    var dia = RandomDay();

                    var sale = new Sale();
                    sale.RegistrationDate = dia;
                    sale.IdTypeDocumentSale = tiposVentas[random.Next(0, cantTipoVentas - 1)].IdTypeDocumentSale;
                    sale.IdTienda = idTienda;
                    sale.IdTurno = turno.IdTurno;
                    sale.IdUsers = idUser;

                    var catProdVenta = random.Next(0, 6);

                    var listDet = new List<DetailSale>();

                    for (int a = 0; a < catProdVenta; a++)
                    {
                        var detalle = new DetailSale();
                        var p = productos[random.Next(0, cantproductos - 1)];
                        if (!listDet.Any(_ => _.IdProduct == p.IdProduct))
                        {
                            detalle.IdProduct = p.IdProduct;
                            detalle.BrandProduct = p.Brand;
                            detalle.CategoryProducty = p.IdCategoryNavigation.Description;
                            detalle.DescriptionProduct = p.Description;
                            detalle.Price = p.Price;
                            detalle.Quantity = random.Next(0, 4);
                            detalle.Total = detalle.Price * detalle.Quantity;
                            listDet.Add(detalle);
                        }
                    }

                    if (listDet.Count() == 0)
                    {
                        catProdVenta = random.Next(0, 6);

                        var detalle = new DetailSale();
                        var p = productos[random.Next(0, cantproductos - 1)];
                        if (!listDet.Any(_ => _.IdProduct == p.IdProduct))
                        {
                            detalle.IdProduct = p.IdProduct;
                            detalle.BrandProduct = p.Brand;
                            detalle.CategoryProducty = p.IdCategoryNavigation.Description;
                            detalle.DescriptionProduct = p.Description;
                            detalle.Price = p.Price;
                            detalle.Quantity = random.Next(0, 4);
                            detalle.Total = detalle.Price * detalle.Quantity;
                            listDet.Add(detalle);
                        }
                    }

                    sale.Total = listDet.Sum(d => d.Total);
                    sale.DetailSales = listDet;

                    if (sale.Total > 0)
                        await _repositorySale.Add(sale);
                }

                return 1;
            }
            catch
            {
                throw;
            }
        }
        public DateTime RandomDay()
        {
            var diaHoy = DateTime.Now;
            var inicioMes = DateTime.Now.Day - 1;
            var diaInicio = diaHoy.AddDays(-inicioMes).AddMonths(-1);

            var gen = new Random();
            int range = (DateTime.Today - diaInicio).Days;

            var day = diaInicio.AddDays(gen.Next(range)).Date;

            var random = new Random();
            var horas = random.Next(0, 13) + 8;
            var minutos = random.Next(0, 60);

            return day.AddHours(horas).AddMinutes(minutos);
        }

    }
}
