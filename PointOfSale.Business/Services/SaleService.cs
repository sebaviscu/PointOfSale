using iText.StyledXmlParser.Jsoup.Safety;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Model.Input;
using PointOfSale.Model.Output;
using System.Globalization;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IGenericRepository<Product> _repositoryProduct;
        private readonly IGenericRepository<Cliente> _repositoryCliente;
        private readonly IGenericRepository<ListaPrecio> _repositoryListaPrecio;
        private readonly IGenericRepository<FacturaEmitida> _repositoryFacturaEmitida;
        private readonly ISaleRepository _repositorySale;
        private readonly IAfipService _afipService;
        private readonly INotificationService _notificationService;
        private readonly IClienteService _clienteService;
        private readonly IAjusteService _ajustesService;
        private readonly ITicketService _ticketService;
        private readonly ICorrelativeNumberService _correlativeNumberService;
        public SaleService(
            IGenericRepository<Product> repositoryProduct,
            ISaleRepository repositorySale,
            IGenericRepository<Cliente> repositoryCliente,
            IGenericRepository<ListaPrecio> repositoryListaPrecio,
            IAfipService afipService,
            IGenericRepository<FacturaEmitida> repositoryFacturaEmitida,
            INotificationService notificationService,
            IClienteService clienteService,
            IAjusteService ajusteService,
            ITicketService ticketService,
            ICorrelativeNumberService correlativeNumberService)
        {
            _repositoryProduct = repositoryProduct;
            _repositorySale = repositorySale;
            _repositoryCliente = repositoryCliente;
            _repositoryListaPrecio = repositoryListaPrecio;
            _afipService = afipService;
            _repositoryFacturaEmitida = repositoryFacturaEmitida;
            _notificationService = notificationService;
            _clienteService = clienteService;
            _ticketService = ticketService;
            _ajustesService = ajusteService;
            _correlativeNumberService = correlativeNumberService;
        }


        public async Task<List<ListaPrecio>> GetProductsSearchAndIdLista(string search, ListaDePrecio listaPrecios)
        {
            var listaProductos = new List<ListaPrecio>();

            if (listaPrecios == ListaDePrecio.Web)
            {
                var queryProductosWeb = _repositoryProduct.QuerySimple();
                queryProductosWeb = queryProductosWeb.Where(_ => _.IsActive);

                if (search.Contains(' '))
                {
                    var split = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var term in split)
                    {
                        var temp = term;
                        queryProductosWeb = queryProductosWeb.Where(p => p.Description.Contains(temp));
                    }
                }
                else
                {
                    queryProductosWeb = queryProductosWeb.Where(p =>
                        p.CodigoBarras.Any(cb => cb.Codigo.Equals(search)) || p.Description.Contains(search));
                }

                var lisProductosWeb = await queryProductosWeb
                    .Include(_ => _.IdCategoryNavigation)
                    .Include(p => p.CodigoBarras)
                    .ToListAsync();

                listaProductos = lisProductosWeb.Select(_ => new ListaPrecio()
                {
                    IdProducto = _.IdProduct,
                    Lista = ListaDePrecio.Web,
                    PorcentajeProfit = 0,
                    Precio = _.PriceWeb.HasValue ? _.PriceWeb.Value : _.Price ?? 0,
                    Producto = _
                }).ToList();
            }
            else
            {
                var queryListaPrecio = _repositoryListaPrecio.QuerySimple();
                queryListaPrecio = queryListaPrecio.Where(p => p.Lista == listaPrecios && p.Producto.IsActive);

                if (search.Contains(' '))
                {
                    var split = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var term in split)
                    {
                        var temp = term;
                        queryListaPrecio = queryListaPrecio.Where(p => p.Producto.Description.Contains(temp));
                    }
                }
                else
                {
                    queryListaPrecio = queryListaPrecio.Where(p =>
                        p.Producto.CodigoBarras.Any(cb => cb.Codigo.Equals(search)) || p.Producto.Description.Contains(search));
                }

                listaProductos = await queryListaPrecio
                    .Include(_ => _.Producto)
                    .ThenInclude(_ => _.IdCategoryNavigation)
                    .Include(_ => _.Producto)
                    .ThenInclude(p => p.CodigoBarras)
                    .ToListAsync();
            }

            return listaProductos;
        }

        public async Task<List<ListaPrecio>> GetProductsSearchAndIdListaWithTags(string search, ListaDePrecio listaPrecios)
        {
            IQueryable<ListaPrecio> queryListaPrecio;

            // Si la búsqueda tiene espacios, dividir términos y crear predicado
            if (search.Contains(' '))
            {
                var split = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var predicate = PredicateBuilder.True<Product>();
                foreach (var term in split)
                {
                    var temp = term;
                    predicate = predicate.And(p => p.Description.Contains(temp));
                }

                // Obtener los productos que coinciden con el predicado
                var idsProdsQuery = await _repositoryProduct.Query(predicate);
                var idsProds = idsProdsQuery.Select(p => p.IdProduct).ToList();

                // Filtrar lista de precios según los productos obtenidos
                queryListaPrecio = await _repositoryListaPrecio.Query(p =>
                    p.Lista == listaPrecios &&
                    p.Producto.IsActive == true &&
                    idsProds.Contains(p.IdProducto));
            }
            else
            {
                // Filtrar por búsqueda directa si no hay espacios
                queryListaPrecio = await _repositoryListaPrecio.Query(p =>
                    p.Lista == listaPrecios &&
                    p.Producto.IsActive == true &&
                    (p.Producto.CodigoBarras.Any(cb => cb.Codigo.Equals(search)) ||
                     p.Producto.Description.Contains(search)));
            }

            // Incluir las relaciones necesarias
            return await queryListaPrecio
                .Include(lp => lp.Producto)
                    .ThenInclude(p => p.IdCategoryNavigation)
                .Include(lp => lp.Producto)
                    .ThenInclude(p => p.CodigoBarras)
                .Include(lp => lp.Producto)
                    .ThenInclude(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                .ToListAsync();
        }


        public async Task<List<Cliente>> GetClientsByFactura(string search)
        {
            IQueryable<Cliente> query = await _repositoryCliente.Query(p =>
           string.Concat(p.Cuil, p.Nombre, p.Direccion).Contains(search));

            return query.Include(_ => _.ClienteMovimientos).ToList();
        }

        public async Task<List<Cliente>> GetClients(string search, int idTienda)
        {
            IQueryable<Cliente> query = await _repositoryCliente.Query(p => p.IsActive && p.IdTienda == idTienda &&
           string.Concat(p.Cuil, p.Nombre).Contains(search));

            return query.Include(_ => _.ClienteMovimientos).ToList();
        }

        public async Task<RegisterSaleOutput> RegisterSale(Sale model, RegistrationSaleInput saleInput)
        {
            var modelResponde = new RegisterSaleOutput();
            Sale sale_created = null;
            try
            {
                if(saleInput.MultiplesFormaDePago.Any(_=>_.FormaDePago == null) && saleInput.ClientId == null)
                {
                    throw new Exception("Es necesario Agregar una forma de pago.");
                }

                var ajustesTask = _ajustesService.GetAjustes(model.IdTienda);
                var lastNumberTask = GetLastSerialNumberSale(model.IdTienda);

                await Task.WhenAll(ajustesTask, lastNumberTask);

                var ajustes = await ajustesTask;
                var lastNumber = await lastNumberTask;
                modelResponde.Ajustes = ajustes;

                var paso = false;

                modelResponde.SaleNumber = lastNumber;
                modelResponde.NombreImpresora = saleInput.ImprimirTicket && !string.IsNullOrEmpty(ajustes.NombreImpresora) ? ajustes.NombreImpresora : null;

                foreach (var m in saleInput.MultiplesFormaDePago)
                {
                    var newVMSale = new Sale
                    {
                        Total = m.Total,
                        IdTypeDocumentSale = m.FormaDePago,
                        IdTurno = model.IdTurno,
                        IdTienda = model.IdTienda,
                        RegistrationUser = model.RegistrationUser,
                        SaleNumber = lastNumber,
                        IsWeb = false,
                        Observaciones = model.Observaciones,
                        TipoFactura = m.TipoFactura
                    };

                    if (!paso)
                    {
                        newVMSale.DetailSales = model.DetailSales;
                        newVMSale.DescuentoRecargo = model.DescuentoRecargo;
                        paso = true;
                    }

                    sale_created = await _repositorySale.Register(newVMSale, ajustes);

                    await _clienteService.RegistrarionClient(sale_created, newVMSale.Total.Value, model.RegistrationUser, model.IdTienda, sale_created.IdSale, saleInput.TipoMovimiento, saleInput.ClientId);
                    
                    if(sale_created.IdClienteMovimiento != null)
                    {
                        _ = await _repositorySale.Edit(sale_created);
                    }

                    //FacturaEmitida facturaEmitida = null;
                    //try
                    //{
                    //    if (ajustes.FacturaElectronica.HasValue && ajustes.FacturaElectronica.Value)
                    //    {
                    //        facturaEmitida = await _afipService.FacturarVenta(sale_created, ajustes, saleInput.CuilFactura, saleInput.IdClienteFactura);

                    //        if (facturaEmitida != null && facturaEmitida.Resultado != "A")
                    //        {
                    //            modelResponde.ErrorFacturacion = facturaEmitida.Observaciones;
                    //        }
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    modelResponde.ErrorFacturacion = e.Message;
                    //    sale_created.ResultadoFacturacion = false;
                    //    await _repositorySale.Edit(sale_created);
                    //}

                    //if (!string.IsNullOrEmpty(modelResponde.ErrorFacturacion))
                    //{
                    //    var notific = new Notifications(sale_created, string.Concat(modelResponde.Errores, modelResponde.ErrorFacturacion));
                    //    await _notificationService.Save(notific);
                    //}

                    //if (saleInput.ImprimirTicket && !string.IsNullOrEmpty(ajustes.NombreImpresora))
                    //{
                    //    var ticket = await RegistrationTicketPrinting(ajustes, sale_created, facturaEmitida);
                    //    if (ticket != null)
                    //    {
                    //        modelResponde.Ticket += ticket.Ticket;
                    //        modelResponde.ImagesTicket.AddRange(ticket.ImagesTicket);
                    //    }
                    //}

                    if (sale_created.IdTypeDocumentSale != null)
                    {
                        if (saleInput.MultiplesFormaDePago.Count == 1)
                        {
                            modelResponde.IdSale = sale_created.IdSale;
                            modelResponde.TipoVenta = "F" + sale_created.TipoFactura.ToString();
                        }
                        else
                        {
                            modelResponde.IdSaleMultiple += $"{sale_created.IdSale},";
                            modelResponde.TipoVenta += $"F{sale_created.TipoFactura.ToString()},";
                        }
                    }
                    else
                    {
                        modelResponde.IdSale = sale_created.IdSale;
                        modelResponde.TipoVenta = "Mov. Cliente";
                    }

                    modelResponde.SaleList.Add(sale_created);
                }

            }
            catch (Exception ex)
            {
                var mensaje = sale_created != null ? "La venta se ha registrado, pero ha habido un error: \n" : "La venta NO SE ha registrado, ha habido un error: \n";
                modelResponde.Errores = mensaje + ex.ToString();
            }
            return modelResponde;
        }

        private async Task<TicketModel?> RegistrationTicketPrinting(Ajustes ajustes, Sale saleCreated, FacturaEmitida? facturaEmitida)
        {
            if (string.IsNullOrEmpty(ajustes.NombreImpresora))
            {
                return null;
            }

            var ticket = await _ticketService.TicketSale(saleCreated, ajustes, facturaEmitida);
            return ticket;
        }

        public async Task<string> GetLastSerialNumberSale(int idTienda)
        {
            return await _correlativeNumberService.GetSerialNumberAndSave(idTienda, "Sale");
        }

        public async Task<List<Sale>> SaleHistory(string saleNumber, string startDate, string endDate, string presupuestos, int idTienda)
        {
            IQueryable<Sale> query = await _repositorySale.Query(_=>_.IdTienda == idTienda);
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
            .Include(dv => dv.DetailSales);


            return await result.ToListAsync();
        }

        public async Task<Sale> Detail(string SaleNumber)
        {
            IQueryable<Sale> query = await _repositorySale.Query(v => v.SaleNumber == SaleNumber);

            return query
               .Include(tdv => tdv.TypeDocumentSaleNavigation)
               .Include(dv => dv.DetailSales)
               .First();
        }

        public async Task<Sale> Edit(Sale entity)
        {
            Sale sale_found = await _repositorySale.Get(c => c.IdSale == entity.IdSale);

            sale_found.IdClienteMovimiento = entity.IdClienteMovimiento;

            bool response = await _repositorySale.Edit(entity);

            if (!response)
                throw new TaskCanceledException("Venta no se pudo cambiar.");

            return sale_found;
        }

        public async Task<Sale> AnularSale(int idSale, string registrationUser)
        {
            Sale sale_found = await _repositorySale.Get(c => c.IdSale == idSale);

            sale_found.IsDelete = true;

            bool response = await _repositorySale.Edit(sale_found);

            var facruturasQuery = await _repositoryFacturaEmitida.Query(v => v.IdSale == idSale);
            var facturas = facruturasQuery.ToList();

            foreach (var f in facturas.Where(_ => _.Resultado == "A"))
            {
                await _afipService.NotaCredito(f.IdFacturaEmitida, registrationUser);
            }

            if (!response)
                throw new TaskCanceledException("Venta no se pudo eliminar.");

            return sale_found;
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
            return await _correlativeNumberService.CreateSerialNumberSale(idTienda);
        }

    }

}

