using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Model.Auditoria;
using PointOfSale.Models;
using PointOfSale.Utilities;
using PointOfSale.Utilities.Response;
using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IRolService _rolService;
        private readonly IDashBoardService _dashboardService;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly IClienteService _clienteService;
        private readonly IProveedorService _proveedorService;
        private readonly IPromocionService _promocionService;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IAjusteService _ajusteService;
        private readonly IAfipService _afipService;
        private readonly ILogger<AdminController> _logger;
        private readonly ITurnoService _turnoService;
        private readonly INotificationService _notificationService;

        public AdminController(
            IDashBoardService dashboardService,
            IUserService userService,
            IRolService rolService,
            ITypeDocumentSaleService typeDocumentSaleService,
            IClienteService clienteService,
            IProveedorService proveedorService,
            IMapper mapper,
            IPromocionService promocionService,
            IProductService productService,
            ICategoryService categoryService,
            IAjusteService ajusteService,
            IAfipService afipService,
            ILogger<AdminController> logger,
            ITurnoService turnoService,
            INotificationService notificationService)
        {
            _dashboardService = dashboardService;
            _userService = userService;
            _rolService = rolService;
            _mapper = mapper;
            _typeDocumentSaleService = typeDocumentSaleService;
            _clienteService = clienteService;
            _proveedorService = proveedorService;
            _promocionService = promocionService;
            _productService = productService;
            _categoryService = categoryService;
            _ajusteService = ajusteService;
            _afipService = afipService;
            _logger = logger;
            _turnoService = turnoService;
            _notificationService = notificationService;
        }

        public IActionResult DashBoard()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
        }

        public IActionResult Users()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(TypeValuesDashboard typeValues, string dateFilter, bool visionGlobal)
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmDashboard = new VMDashBoard();

                var ejeXint = new int[0];
                var ejeX = new string[0];
                var dateActual = SetDate(typeValues, dateFilter);
                DateTime dateCompare;
                var textoFiltroDiaSemanaMes = string.Empty;

                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        dateCompare = dateActual.AddDays(-1);
                        vmDashboard.Actual = "Hoy";
                        vmDashboard.Anterior = "Ayer";
                        vmDashboard.EjeXLeyenda = "Horas";
                        textoFiltroDiaSemanaMes = dateActual.Date.ToShortDateString();
                        break;

                    case TypeValuesDashboard.Semana:
                        ejeXint = new int[7];
                        ejeX = new string[7];
                        dateCompare = dateActual.AddDays(-7);

                        for (int i = 0; i < 7; i++)
                        {
                            ejeXint[i] = i + 1;
                            ejeX[i] = ((DiasSemana)i + 1).ToString();
                        }

                        vmDashboard.Actual = "Semana actual";
                        vmDashboard.Anterior = "Semana pasada";
                        vmDashboard.EjeXLeyenda = "Dias";

                        var weekInt = (int)dateActual.DayOfWeek != 0 ? (int)dateActual.DayOfWeek : 7;
                        var fechaString = dateActual.AddDays(-(weekInt - 1));

                        textoFiltroDiaSemanaMes = $"{fechaString.ToShortDateString()} - {fechaString.AddDays(6).ToShortDateString()}";
                        break;

                    case TypeValuesDashboard.Mes:
                        var cantDaysInMonth = DateTime.DaysInMonth(dateActual.Date.Year, dateActual.Date.Month);
                        ejeXint = new int[cantDaysInMonth];
                        ejeX = new string[cantDaysInMonth];
                        dateCompare = dateActual.AddMonths(-1);

                        for (int i = 0; i < cantDaysInMonth; i++)
                        {
                            ejeXint[i] = i;
                            ejeX[i] = (i + 1).ToString();
                        }
                        vmDashboard.Actual = "Mes actual";
                        vmDashboard.Anterior = "Mes pasado";
                        vmDashboard.EjeXLeyenda = "Dias";

                        DateTimeFormatInfo dtinfo = new CultureInfo("es-ES", false).DateTimeFormat;
                        textoFiltroDiaSemanaMes = dtinfo.GetMonthName(dateActual.Month);

                        break;
                    default:
                        dateCompare = dateActual;
                        break;
                }

                List<VMSalesWeek> listSales = new List<VMSalesWeek>();
                List<VMSalesWeek> listSalesComparacion = new List<VMSalesWeek>();

                var resultados = await _dashboardService.GetSales(typeValues, user.IdTienda, dateActual, visionGlobal);

                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        ProcesarVentasHoy(resultados.VentasActualesHour, resultados.VentasComparacionHour, out listSales, out listSalesComparacion, out ejeX);
                        break;

                    case TypeValuesDashboard.Semana:
                        listSales = GetSalesComparacionWeek(dateActual, resultados.VentasActuales, dateFilter != null);
                        listSalesComparacion = GetSalesComparacionWeek(dateCompare, resultados.VentasComparacion, true);
                        break;

                    case TypeValuesDashboard.Mes:
                        listSales = GetSalesComparacionMonth(dateActual, resultados.VentasActuales);
                        listSalesComparacion = GetSalesComparacionMonth(dateCompare, resultados.VentasComparacion);
                        break;
                }

                vmDashboard.TextoFiltroDiaSemanaMes = textoFiltroDiaSemanaMes;
                vmDashboard.EjeX = ejeX;
                vmDashboard.SalesList = listSales.Select(_ => (int)_.Total).ToList();
                vmDashboard.SalesListComparacion = listSalesComparacion.Select(_ => (int)_.Total).ToList();
                vmDashboard.TotalSales = listSales.Sum(_ => _.Total).ToString("F0");
                vmDashboard.TotalSalesComparacion = "$ " + listSalesComparacion.Sum(_ => _.Total).ToString("F0");
                vmDashboard.CantidadClientes = resultados.CantidadClientes;

                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de dashboard", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }
        }

        private void ProcesarVentasHoy(Dictionary<int, decimal> ventasHoy, Dictionary<int, decimal> ventasAyer, out List<VMSalesWeek> ventasHoyArray, out List<VMSalesWeek> ventasAyerArray, out string[] ejeX)
        {
            var horasHoy = ventasHoy.Select(v => v.Key).ToList();
            var horasAyer = ventasAyer.Select(v => v.Key).ToList();
            var todasLasHoras = horasHoy.Union(horasAyer).Distinct().OrderBy(h => h).ToList();

            int horaMinima = todasLasHoras.Min();
            int horaMaxima = todasLasHoras.Max();

            var horasConsecutivas = Enumerable.Range(horaMinima, horaMaxima - horaMinima + 1).ToList();

            var ventasHoyList = new List<VMSalesWeek>();
            var ventasAyerList = new List<VMSalesWeek>();

            ejeX = horasConsecutivas.Select(h => h.ToString()).ToArray();

            foreach (var hora in horasConsecutivas)
            {
                // Ventas de hoy para esta hora
                var ventaHoy = ventasHoy.ContainsKey(hora) ? ventasHoy[hora] : 0;
                ventasHoyList.Add(new VMSalesWeek { Total = ventaHoy });

                // Ventas de ayer para esta hora
                var ventaAyer = ventasAyer.ContainsKey(hora) ? ventasAyer[hora] : 0;
                ventasAyerList.Add(new VMSalesWeek { Total = ventaAyer });
            }

            ventasHoyArray = ventasHoyList;
            ventasAyerArray = ventasAyerList;
        }


        public async Task<IActionResult> GetGastosSueldos(TypeValuesDashboard typeValues, string dateFilter, bool visionGlobal)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);

                var gastosSueldosList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetGastosSueldos(typeValues, user.IdTienda, dateActual, visionGlobal))
                {
                    gastosSueldosList.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });

                };
                var totGastosSueldos = gastosSueldosList.Sum(_ => _.Total);

                var vmDashboard = new VMDashBoard();
                vmDashboard.GastosSueldosTexto = totGastosSueldos.ToString("F0");
                vmDashboard.GastosPorTipoSueldos = gastosSueldosList;

                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de sueldos", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }

        }

        public async Task<IActionResult> GetSalesByTypoVentaByGrafico(TypeValuesDashboard typeValues, string dateFilter, bool visionGlobal)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();

                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetSalesByTypoVenta(typeValues, user.IdTienda, dateActual, visionGlobal))
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }

                var vmDashboard = new VMDashBoard();
                vmDashboard.VentasPorTipoVenta = VentasPorTipoVenta;

                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de ventas por forma de pago", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }
        }

        public async Task<IActionResult> GetGastos(TypeValuesDashboard typeValues, string dateFilter, bool visionGlobal)
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosParticualresList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetGastos(typeValues, user.IdTienda, dateActual, visionGlobal))
                {
                    gastosParticualresList.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                };
                var totGastosParticualres = gastosParticualresList.Sum(_ => _.Total);

                var vmDashboard = new VMDashBoard();
                vmDashboard.GastosPorTipo = gastosParticualresList;
                vmDashboard.GastosTexto = totGastosParticualres.ToString("F0");


                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de gastos", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }
        }

        public async Task<IActionResult> GetMovimientosProveedores(TypeValuesDashboard typeValues, string dateFilter, bool visionGlobal)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosProveedores = new List<VMVentasPorTipoDeVenta>();
                var movimientosProv = await _dashboardService.GetMovimientosProveedores(typeValues, user.IdTienda, dateActual, visionGlobal);
                var gastosProvTotales = movimientosProv.Sum(_ => _.Value);

                foreach (var item in movimientosProv)
                {
                    gastosProveedores.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                };

                var vmDashboard = new VMDashBoard();
                vmDashboard.GastosPorTipoProveedor = gastosProveedores;
                vmDashboard.GastosProvvedoresTexto = gastosProvTotales.ToString("F0");


                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de pagos de proveedores", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }

        }

        public DateTime SetDate(TypeValuesDashboard typeValues, string dateFilter)
        {
            var dateActual = TimeHelper.GetArgentinaTime();

            if (!string.IsNullOrEmpty(dateFilter))
            {
                var dateSplit = dateFilter.Split('/');
                dateActual = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]));
            }

            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:
                    dateActual = dateActual.Date;
                    break;
                case TypeValuesDashboard.Semana:
                    int daysToSubtract = (int)dateActual.DayOfWeek - 1;
                    if (daysToSubtract < 0)
                    {
                        daysToSubtract = 6;
                    }
                    dateActual = dateActual.AddDays(-daysToSubtract);
                    break;
                case TypeValuesDashboard.Mes:
                    dateActual = new DateTime(dateActual.Year, dateActual.Month, 1);
                    break;
            }

            return dateActual;
        }


        [HttpGet]
        public async Task<IActionResult> GetSalesByTypoVenta(TypeValuesDashboard typeValues, string idCategoria, string dateFilter, bool visionGlobal)
        {
            var gResponse = new GenericResponse<List<VMProductsWeek>>();

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

                var ProductListWeek = new List<VMProductsWeek>();
                int i = 0;
                var dateActual = TimeHelper.GetArgentinaTime();
                if (dateFilter != null)
                {
                    var dateSplit = dateFilter.Split('/');
                    dateActual = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]), 0, 0, 0);
                }

                var datos = await _dashboardService.ProductsTopByCategory(typeValues, idCategoria, tiendaId, dateActual, visionGlobal);

                if (datos!= null && datos.Any())
                {
                    var prodsId = datos.Select(_ => _.IdProduct).ToList();

                    var prods = await _productService.ListByIds(prodsId);

                    foreach (var item in datos)
                    {
                        var prod = prods.FirstOrDefault(_ => _.IdProduct == item.IdProduct);
                        if (prod != null)
                        {
                            var descr = item.Key;
                            if (item.Key.Length > 23)
                            {
                                descr = item.Key.Substring(0, 20) + "...";
                            }

                                ProductListWeek.Add(new VMProductsWeek()
                            {
                                Product = $"{++i}. {descr} ",
                                Quantity = $" {item.Total} {(prod.TipoVenta == Model.Enum.TipoVenta.U ? "U." : prod.TipoVenta)}"
                            });
                        }

                    }
                }

                gResponse.State = true;
                gResponse.Object = ProductListWeek;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar el top ventas de dashboard", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter), ("IdCategoria", idCategoria));
            }

        }

        public async Task<IActionResult> GetGastosByTienda(TypeValuesDashboard typeValues, string dateFilter)
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosParticualresList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetGastosByTienda(typeValues, dateActual))
                {
                    gastosParticualresList.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                };
                var totGastosParticualres = gastosParticualresList.Sum(_ => _.Total);

                var vmDashboard = new VMDashBoard();
                vmDashboard.GastosPorTipo = gastosParticualresList;
                vmDashboard.GastosTexto = totGastosParticualres.ToString("F0");


                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de gastos venta global", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }
        }

        public async Task<IActionResult> GetMovimientosProveedoresByTienda(TypeValuesDashboard typeValues, string dateFilter)
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosParticualresList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetMovimientosProveedoresByTienda(typeValues, dateActual))
                {
                    gastosParticualresList.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                };
                var totGastosParticualres = gastosParticualresList.Sum(_ => _.Total);

                var vmDashboard = new VMDashBoard();
                vmDashboard.GastosPorTipo = gastosParticualresList;
                vmDashboard.GastosTexto = totGastosParticualres.ToString("F0");


                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de proveedores en venta global", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }
        }
        public async Task<IActionResult> GetSalesByTypoVentaByTienda(TypeValuesDashboard typeValues, string dateFilter)
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosParticualresList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetSalesByTypoVentaByTienda(typeValues, dateActual))
                {
                    gastosParticualresList.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                };
                var totGastosParticualres = gastosParticualresList.Sum(_ => _.Total);

                var vmDashboard = new VMDashBoard();
                vmDashboard.GastosPorTipo = gastosParticualresList;
                vmDashboard.GastosTexto = totGastosParticualres.ToString("F0");


                gResponse.State = true;
                gResponse.Object = vmDashboard;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar los datos de ventas en venta global", _logger, model: null, ("TypeValues", typeValues), ("DateFilter", dateFilter));
            }
        }

        private static List<VMSalesWeek> GetSalesComparacionWeek(DateTime fechaInicio, Dictionary<DateTime, decimal> resultados, bool semanaCompleta)
        {
            var lis = new List<VMSalesWeek>();
            var fechaInicioSemana = fechaInicio.Date;
            var hoy = DateTime.Today.Date;

            int diasSemana;
            if (semanaCompleta)
            {
                diasSemana = 7;
            }
            else
            {
                diasSemana = (int)(hoy - fechaInicioSemana).TotalDays + 1;
            }

            for (var i = 0; i < diasSemana; i++)
            {
                var fechaActual = fechaInicioSemana.AddDays(i);
                if (resultados.TryGetValue(fechaActual, out decimal valor))
                {
                    lis.Add(new VMSalesWeek { Total = valor });
                }
                else
                {
                    lis.Add(new VMSalesWeek { Total = 0m });
                }
            }

            return lis;
        }



        private static List<VMSalesWeek> GetSalesComparacionMonth(DateTime dateStart, Dictionary<DateTime, decimal> resultados)
        {
            var lis = new List<VMSalesWeek>();
            var cantDaysInMonth = DateTime.DaysInMonth(dateStart.Year, dateStart.Month);

            for (var i = 0; i < cantDaysInMonth; i++)
            {
                var fechaActual = new DateTime(dateStart.Year, dateStart.Month, i + 1);
                if (resultados.TryGetValue(fechaActual, out decimal valor))
                {
                    lis.Add(new VMSalesWeek { Total = valor });
                }
                else
                {
                    lis.Add(new VMSalesWeek { Total = 0m });
                }
            }

            return lis;
        }


        private static List<VMSalesWeek> GetSalesComparacionHour(int[] ejeXint, DateTime dateCompare, GraficoVentasConComparacion resultados)
        {

            var lis = new List<VMSalesWeek>();

            for (int x = 0; x < ejeXint.Length; x++)
            {
                var item = resultados.VentasComparacionHour.FirstOrDefault(_ => _.Key == ejeXint[x]);
                if (item.Value == 0)
                {
                    lis.Add(new VMSalesWeek() { Total = 0m });
                }
                else
                {
                    lis.Add(new VMSalesWeek() { Total = item.Value });
                }
            }
            return lis;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var gResponse = new GenericResponse<List<VMRol>>();

            try
            {
                gResponse.Object = _mapper.Map<List<VMRol>>(await _rolService.List());
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar roles", _logger);
            }
        }

        /// <summary>
        /// Devuelve los usuarios para un DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                List<VMUser> listUsers = _mapper.Map<List<VMUser>>(await _userService.List());
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar los usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByTienda()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);

            try
            {
                List<VMUser> listUsers = _mapper.Map<List<VMUser>>(await _userService.GetAllUsersByTienda(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar los usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByRolByTienda(int idRol)
        {
            var user = ValidarAutorizacion([Roles.Administrador]);

            try
            {
                List<VMUser> listUsers = _mapper.Map<List<VMUser>>(await _userService.GetUsersByRolByTienda(idRol, user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar los usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        /// <summary>
        /// Devuelve los usuarios para un DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUser(int idUser)
        {
            var gResponse = new GenericResponse<VMUser>();

            try
            {
                var user = _mapper.Map<VMUser>(await _userService.GetById(idUser));
                user.Password = EncryptionHelper.DecryptString(user.Password);

                gResponse.Object = user;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar un usuario", _logger, idUser);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string horarios)
        {

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmUser = JsonConvert.DeserializeObject<VMUser>(model);
                var vmHorarios = JsonConvert.DeserializeObject<List<VMHorario>>(horarios);

                foreach (var h in vmHorarios)
                {
                    h.RegistrationDate = TimeHelper.GetArgentinaTime();
                    h.RegistrationUser = user.UserName;
                }

                vmUser.Horarios = vmHorarios;

                if (vmUser.Email.ToLower() == "admin")
                {
                    throw new Exception("El usuario no puede ser 'admin'");
                }

                vmUser.IdTienda = user.IdTienda;
                vmUser.Password = EncryptionHelper.EncryptString(vmUser.Password);
                if (photo != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        photo.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        vmUser.Photo = fileBytes;
                    }
                }
                else
                    vmUser.Photo = null;


                User usuario_creado = await _userService.Add(_mapper.Map<User>(vmUser));

                gResponse.Object = _mapper.Map<VMUser>(usuario_creado); ;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear usuarios", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string horarios)
        {

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                VMUser vmUser = JsonConvert.DeserializeObject<VMUser>(model);

                var vmHorarios = JsonConvert.DeserializeObject<List<VMHorario>>(horarios);

                foreach (var h in vmHorarios)
                {
                    h.ModificationUser = user.UserName;
                    h.ModificationDate = TimeHelper.GetArgentinaTime();
                    h.RegistrationDate = TimeHelper.GetArgentinaTime();
                }
                vmUser.Horarios = vmHorarios;

                if (vmUser.Email.ToLower() == "admin")
                {
                    throw new Exception("El usuario no puede ser 'admin'");
                }

                vmUser.ModificationUser = user.UserName;
                vmUser.Password = EncryptionHelper.EncryptString(vmUser.Password);
                if (photo != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        photo.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        vmUser.Photo = fileBytes;
                    }
                }

                User user_edited = await _userService.Edit(_mapper.Map<User>(vmUser));

                gResponse.Object = _mapper.Map<VMUser>(user_edited); ;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar usuarios", _logger, model);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int IdUser)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _userService.Delete(IdUser);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al borrar usuarios", _logger, IdUser);
            }

        }

        public IActionResult Cliente()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
        }

        /// <summary>
        /// Recupero cliente para DataTable y select2
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCliente()
        {
            var gResponse = new GenericResponse<List<VMCliente>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var listUsers = _mapper.Map<List<VMCliente>>(await _clienteService.List(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear clientes", _logger);
            }

        }

        /// <summary>
        /// Recupera movimientos de cliente para DataTabe
        /// </summary>
        /// <param name="idCliente"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMovimientoCliente(int idCliente)
        {
            if (!ModelState.IsValid)
            {
                return View(idCliente);
            }
            var gResponse = new GenericResponse<List<VMClienteMovimiento>>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var listUsers = _mapper.Map<List<VMClienteMovimiento>>(await _clienteService.ListMovimientoscliente(idCliente, user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar movimientos de clientes", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCliente([FromBody] VMCliente model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var gResponse = new GenericResponse<VMCliente>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                model.IdTienda = user.IdTienda;
                var usuario_creado = await _clienteService.Add(_mapper.Map<Cliente>(model));

                gResponse.Object = _mapper.Map<VMCliente>(usuario_creado);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear cliente", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateCliente([FromBody] VMCliente model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var gResponse = new GenericResponse<VMCliente>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.ModificationUser = user.UserName;
                var user_edited = await _clienteService.Edit(_mapper.Map<Cliente>(model));

                gResponse.Object = _mapper.Map<VMCliente>(user_edited);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar cliente", _logger, model);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCliente(int idCliente)
        {
            if (!ModelState.IsValid)
            {
                return View(idCliente);
            }


            var gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _clienteService.Delete(idCliente);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar cliente", _logger, idCliente);
            }

        }

    }
}

