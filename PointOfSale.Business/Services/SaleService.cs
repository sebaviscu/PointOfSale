using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System.ComponentModel;
using System.Globalization;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class SaleService : ISaleService
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        private readonly IGenericRepository<Product> _repositoryProduct;
        private readonly IGenericRepository<Cliente> _repositoryCliente;
        private readonly IGenericRepository<ListaPrecio> _repositoryListaPrecio;
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
            ITurnoService turnoService,
            IGenericRepository<ListaPrecio> repositoryListaPrecio)
        {
            _repositoryProduct = repositoryProduct;
            _repositorySale = repositorySale;
            _repositoryCliente = repositoryCliente;
            _rTypeNumber = rTypeNumber;
            _rProduct = rProduct;
            _turnoService = turnoService;
            _repositoryListaPrecio = repositoryListaPrecio;
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

                list = query.Include(c => c.IdCategoryNavigation).Include(_ => _.ListaPrecios).ToList();

            }

            return list;
        }

        public async Task<List<Product>> GetProductsSearchAndIdLista(string search, ListaDePrecio listaPrecios)
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
                var idsProds = _repositoryProduct.SqlRaw(query).Select(_ => _.IdProduct).ToList();

                var queryProducts = await _repositoryListaPrecio.Query(p =>
                           p.Lista == listaPrecios &&
                           p.Producto.IsActive == true &&
                           idsProds.Contains(p.IdProducto));

                list = queryProducts.Include(c => c.Producto).ToList().Select(_ => _.Producto).ToList();
            }
            else
            {
                IQueryable<ListaPrecio> query = await _repositoryListaPrecio.Query(p =>
                            p.Lista == listaPrecios &&
                            p.Producto.IsActive == true &&
                            string.Concat(p.Producto.BarCode, p.Producto.Description).Contains(search));

                list = query.Include(c => c.Producto).ToList().Select(_ => _.Producto).ToList();
            }

            return list;
        }

        public async Task<List<Cliente>> GetClients(string search)
        {
            IQueryable<Cliente> query = await _repositoryCliente.Query(p =>
           string.Concat(p.Cuit, p.Nombre).Contains(search));

            return query.Include(_ => _.ClienteMovimientos).ToList();
        }

        public async Task<Sale> Register(Sale entity)
        {
            try
            {
                var sale = await _repositorySale.Register(entity);
                return sale;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Sale>> SaleHistory(string SaleNumber, string StarDate, string EndDate, string presupuestos)
        {
            IQueryable<Sale> query = await _repositorySale.Query();
            StarDate = StarDate is null ? "" : StarDate;
            EndDate = EndDate is null ? "" : EndDate;

            IQueryable<Sale> result;

            if (StarDate != "" && EndDate != "")
            {

                DateTime start_date = DateTime.ParseExact(StarDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime end_date = DateTime.ParseExact(EndDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

                result = query.Where(v =>
                    v.RegistrationDate.Value.Date >= start_date.Date &&
                    v.RegistrationDate.Value.Date <= end_date.Date
                )
                .Include(tdv => tdv.TypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .OrderByDescending(_ => _.IdSale);

                switch (presupuestos)
                {
                    case "incluir":

                        break;
                    case "noIncluir":
                        result = result.Where(_ => _.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu);
                        break;
                    case "solamente":
                        result = result.Where(_ => _.TypeDocumentSaleNavigation.TipoFactura == TipoFactura.Presu);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                result = query.Where(v => v.SaleNumber.EndsWith(SaleNumber))
                .Include(tdv => tdv.TypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales)
                .OrderByDescending(_ => _.IdSale);
            }



            return result.ToList();
        }

        public async Task<Sale> Detail(string SaleNumber)
        {
            IQueryable<Sale> query = await _repositorySale.Query(v => v.SaleNumber == SaleNumber);

            return query
               .Include(tdv => tdv.TypeDocumentSaleNavigation)
               .Include(u => u.IdUsersNavigation)
               .Include(dv => dv.DetailSales)
               .First();
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

        public async Task<bool> GenerarVentas(int idTienda, int idUser)
        {

            var tiposVentas = await _rTypeNumber.List();
            var productos = await _rProduct.List();
            var turno = await _turnoService.GetTurnoActual(idTienda);

            var random = new Random();
            var cantTipoVentas = tiposVentas.Count;
            var cantproductos = productos.Count;

            if (cantproductos < 4)
            { 
                return false; 
            }

            try
            {
                for (int i = 0; i < 400; i++) /*1200*/
                {
                    var dia = RandomDayMes();

                    var sale = new Sale();
                    sale.RegistrationDate = dia;
                    await CrearVentaGenerada(idTienda, idUser, tiposVentas, productos, turno, random, cantTipoVentas, cantproductos, sale);
                }

                for (int i = 0; i < 50; i++) // ayer y hoy
                {
                    var dia = RandomDayDias();

                    var sale = new Sale();
                    sale.RegistrationDate = dia;
                    await CrearVentaGenerada(idTienda, idUser, tiposVentas, productos, turno, random, cantTipoVentas, cantproductos, sale);
                }

                return true;
            }
            catch
            {
                return false;
                throw;
            }
        }

        private async Task CrearVentaGenerada(int idTienda, int idUser, List<TypeDocumentSale> tiposVentas, List<Product> productos, Turno turno, Random random, int cantTipoVentas, int cantproductos, Sale sale)
        {
            sale.IdTypeDocumentSale = tiposVentas[random.Next(0, cantTipoVentas - 1)].IdTypeDocumentSale;
            sale.IdTienda = idTienda;
            sale.IdTurno = turno.IdTurno;
            sale.IdUsers = idUser;

            var catProdVenta = random.Next(0, 4);

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
                catProdVenta = random.Next(0, 4);

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

        public DateTime RandomDayMes()
        {
            var diaHoy = DateTimeNowArg;
            var inicioMes = DateTimeNowArg.Day - 1;
            var diaInicio = diaHoy.AddDays(-inicioMes).AddMonths(-1);

            var gen = new Random();
            int range = (DateTime.Today - diaInicio).Days;

            var day = diaInicio.AddDays(gen.Next(range)).Date;

            var random = new Random();
            var horas = random.Next(0, 13) + 8;
            var minutos = random.Next(0, 60);

            return day.AddHours(horas).AddMinutes(minutos);
        }
        public DateTime RandomDayDias()
        {
            var diaInicio = DateTimeNowArg.AddDays(-1);

            var gen = new Random();
            int range = (DateTime.Today - diaInicio).Days;

            var day = diaInicio.AddDays(gen.Next(2)).Date;

            var random = new Random();
            var horas = random.Next(0, 13) + 8;
            var minutos = random.Next(0, 60);

            return day.AddHours(horas).AddMinutes(minutos);
        }

        public async Task<Sale> GetSale(int idSale)
        {
            var query = await _repositorySale.Query(v => v.IdSale == idSale);

            return query.Include(dv => dv.DetailSales).FirstOrDefault();
        }


        public async Task<Sale> Edit(int idSale, int formaPago)
        {
            try
            {
                var sale = await _repositorySale.Get(c => c.IdSale == idSale);

                sale.RegistrationDate = DateTimeNowArg;
                sale.IdTypeDocumentSale = formaPago;

                bool response = await _repositorySale.Edit(sale);

                if (!response)
                    throw new TaskCanceledException("Sale no se pudo cambiar.");

                return sale;
            }
            catch
            {
                throw;
            }
        }

    }

}

