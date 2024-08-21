using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System.Globalization;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class SaleService : ISaleService
    {
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

            // Si la búsqueda contiene espacios, dividimos la búsqueda en múltiples términos
            if (search.Contains(' '))
            {
                var split = search.Split(' ');

                // Construcción de la consulta SQL con LIKE para cada término de búsqueda
                var query = "SELECT * FROM Product WHERE ";
                for (int i = 0; i < split.Length; i++)
                {
                    query += $"description LIKE '%{split[i]}%' ";
                    if (i < split.Length - 1) query += " AND ";
                }

                // Ejecutar la consulta con SQL crudo (mejorable usando parámetros SQL para evitar inyecciones)
                list = _repositoryProduct.SqlRaw(query).ToList();
            }
            else
            {
                // Usar IQueryable con Entity Framework para optimizar la consulta
                var query = await _repositoryProduct.Query(p =>
                               p.IsActive == true &&
                               (p.BarCode.Contains(search) || p.Description.Contains(search)));

                // Incluir las propiedades de navegación necesarias
                list = await query.Include(c => c.IdCategoryNavigation)
                                  .Include(p => p.ListaPrecios)
                                  .ToListAsync();
            }

            return list;
        }


        public async Task<List<ListaPrecio>> GetProductsSearchAndIdLista(string search, ListaDePrecio listaPrecios)
        {
            var list = new List<Product>();
            IQueryable<Product> queryProducts;
            IQueryable<ListaPrecio> queryListaPrecio;

            if (search.Contains(' '))
            {
                var split = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var predicate = PredicateBuilder.True<Product>();
                foreach (var term in split)
                {
                    var temp = term;
                    predicate = predicate.And(p => p.Description.Contains(temp));
                }
                var idsProdsQuery = await _repositoryProduct.Query(predicate);
                var idsProds = idsProdsQuery.Select(p => p.IdProduct).ToList();

                queryListaPrecio = await _repositoryListaPrecio.Query(p =>
                    p.Lista == listaPrecios &&
                    p.Producto.IsActive == true &&
                    idsProds.Contains(p.IdProducto));
            }
            else
            {
                queryListaPrecio = await _repositoryListaPrecio.Query(p =>
                    p.Lista == listaPrecios &&
                    p.Producto.IsActive == true &&
                    (p.Producto.BarCode.Contains(search) || p.Producto.Description.Contains(search)));
            }

            return queryListaPrecio.Include(_ => _.Producto).ThenInclude(_ => _.IdCategoryNavigation).ToList();
        }


        public async Task<List<Product>> GetProductsSearchAndIdLista2(string search, ListaDePrecio listaPrecios)
        {
            var list = new List<Product>();
            IQueryable<Product> queryProducts;

            if (search.Contains(' '))
            {
                var split = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Construir la expresión de búsqueda dinámica
                var predicate = PredicateBuilder.True<Product>();
                foreach (var term in split)
                {
                    var temp = term; // Necesario para evitar problemas con la clausura de variables
                    predicate = predicate.And(p => p.Description.Contains(temp));
                }

                queryProducts = await _repositoryProduct.Query(predicate);
            }
            else
            {
                queryProducts = await _repositoryProduct.Query(p =>
                    p.IsActive == true &&
                    (p.BarCode.Contains(search) || p.Description.Contains(search)));
            }

            // Obtener los Ids de productos resultantes de la búsqueda
            var productIds = await queryProducts.Select(p => p.IdProduct).ToListAsync();

            // Obtener las listas de precios para los productos resultantes
            var listaPreciosQuery = await _repositoryListaPrecio.Query(p =>
                p.Lista == listaPrecios &&
                productIds.Contains(p.IdProducto));

            var listaPrecios2 = await listaPreciosQuery
                .Include(p => p.Producto)
                .ToListAsync();

            // Filtrar productos con ListaPrecios igual a 1
            list = listaPreciosQuery
                .Where(lp => lp.Lista == listaPrecios)
                .Select(lp => lp.Producto)
                .Distinct()
                .ToList();

            return list;
        }


        public async Task<List<Cliente>> GetClientsByFactura(string search)
        {
            IQueryable<Cliente> query = await _repositoryCliente.Query(p =>
           string.Concat(p.Cuil, p.Nombre, p.Direccion).Contains(search));

            return query.Include(_ => _.ClienteMovimientos).ToList();
        }

        public async Task<List<Cliente>> GetClients(string search)
        {
            IQueryable<Cliente> query = await _repositoryCliente.Query(p => p.IsActive &&
           string.Concat(p.Cuil, p.Nombre).Contains(search));

            return query.Include(_ => _.ClienteMovimientos).ToList();
        }

        public async Task<Sale> Register(Sale entity, Ajustes ajustes)
        {
            var sale = await _repositorySale.Register(entity, ajustes);
            return sale;
        }

        public async Task<string> GetLastSerialNumberSale(int idTienda)
        {
            return await _repositorySale.GetLastSerialNumberSale(idTienda);
        }

        public async Task<List<Sale>> SaleHistory(string saleNumber, string startDate, string endDate, string presupuestos)
        {
            IQueryable<Sale> query = await _repositorySale.Query(); // Eliminado el await innecesario
            startDate = startDate ?? "";
            endDate = endDate ?? "";

            IQueryable<Sale> result;

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime start_date = DateTime.ParseExact(startDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime end_date = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

                result = query.Where(v =>
                    v.RegistrationDate.Value.Date >= start_date.Date &&
                    v.RegistrationDate.Value.Date <= end_date.Date
                )
                .OrderByDescending(_ => _.IdSale)
                .Include(tdv => tdv.TypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales);

                switch (presupuestos)
                {
                    case "noIncluir":
                        result = result.Where(_ => _.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu);
                        break;
                    case "solamente":
                        result = result.Where(_ => _.TypeDocumentSaleNavigation.TipoFactura == TipoFactura.Presu);
                        break;
                }
            }
            else
            {
                result = query.Where(v => v.SaleNumber.EndsWith(saleNumber))
                .OrderByDescending(_ => _.IdSale)
                .Include(tdv => tdv.TypeDocumentSaleNavigation)
                .Include(u => u.IdUsersNavigation)
                .Include(dv => dv.DetailSales);
            }

            return await result.ToListAsync(); // Usar ToListAsync para evitar bloquear el hilo
        }

        public async Task<List<Sale>> HistoryTurnoActual(int idTurno)
        {
            IQueryable<Sale> query = await _repositorySale.Query();

            var result = query.Where(v => v.IdTurno == idTurno)
            .OrderByDescending(_ => _.IdSale)
            .Include(tdv => tdv.TypeDocumentSaleNavigation)
            .Include(u => u.IdUsersNavigation)
            .Include(dv => dv.DetailSales);


            return await result.ToListAsync();
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
            var productos = await _rProduct.ListActive();
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
            sale.IsWeb = false;

            var catProdVenta = random.Next(0, 4);

            var listDet = new List<DetailSale>();

            for (int a = 0; a < catProdVenta; a++)
            {
                var detalle = new DetailSale();
                var p = productos[random.Next(0, cantproductos - 1)];
                if (!listDet.Any(_ => _.IdProduct == p.IdProduct))
                {
                    detalle.IdProduct = p.IdProduct;
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
            var diaHoy = TimeHelper.GetArgentinaTime();
            var inicioMes = TimeHelper.GetArgentinaTime().Day - 1;
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
            var diaInicio = TimeHelper.GetArgentinaTime().AddDays(-1);

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

                sale.RegistrationDate = TimeHelper.GetArgentinaTime();
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

        public async Task<CorrelativeNumber> CreateSerialNumberSale(int idTienda)
        {
            return await _repositorySale.CreateSerialNumberSale(idTienda);
        }

    }

}

