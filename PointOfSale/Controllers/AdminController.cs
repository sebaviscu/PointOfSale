using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using static iTextSharp.text.pdf.AcroFields;
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
            ILogger<AdminController> logger)
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
        }

        public IActionResult DashBoard()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        public IActionResult Users()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(TypeValuesDashboard typeValues, string dateFilter)
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

                var resultados = await _dashboardService.GetSales(typeValues, user.IdTienda, dateActual);

                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        listSales = resultados.VentasActualesHour.Select(v => new VMSalesWeek { Total = v.Value }).ToList();
                        listSalesComparacion = resultados.VentasComparacionHour.Select(v => new VMSalesWeek { Total = v.Value }).ToList();
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
                var errorMessage = "Error al recuperar los datos de dashboard";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. TypeValues: {TypeValues}. DateFilter: {DateFilter}", errorMessage, typeValues.ToJson(), dateFilter.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        public async Task<IActionResult> GetGastosSueldos(TypeValuesDashboard typeValues, string dateFilter)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);

                var gastosSueldosList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetGastosSueldos(typeValues, user.IdTienda, dateActual))
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
                var errorMessage = "Error al recuperar los datos de sueldos";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. TypeValues: {TypeValues}. DateFilter: {DateFilter}", errorMessage, typeValues.ToJson(), dateFilter.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public async Task<IActionResult> GetSalesByTypoVentaByGrafico(TypeValuesDashboard typeValues, string dateFilter)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();

                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetSalesByTypoVenta(typeValues, user.IdTienda, dateActual))
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
                var errorMessage = "Error al recuperar los datos de ventas por forma de pago";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. TypeValues: {TypeValues}. DateFilter: {DateFilter}", errorMessage, typeValues.ToJson(), dateFilter.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public async Task<IActionResult> GetGastos(TypeValuesDashboard typeValues, string dateFilter)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosParticualresList = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetGastos(typeValues, user.IdTienda, dateActual))
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
                var errorMessage = "Error al recuperar los datos de gastos";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. TypeValues: {TypeValues}. DateFilter: {DateFilter}", errorMessage, typeValues.ToJson(), dateFilter.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public async Task<IActionResult> GetMovimientosProveedoresByTienda(TypeValuesDashboard typeValues, string dateFilter)
        {

            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var dateActual = SetDate(typeValues, dateFilter);
                var gastosProveedores = new List<VMVentasPorTipoDeVenta>();
                var movimientosProv = await _dashboardService.GetMovimientosProveedoresByTienda(typeValues, user.IdTienda, dateActual);
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
                var errorMessage = "Error al recuperar los datos de pagos de proveedores";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. TypeValues: {TypeValues}. DateFilter: {DateFilter}", errorMessage, typeValues.ToJson(), dateFilter.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
        public async Task<IActionResult> GetSalesByTypoVenta(TypeValuesDashboard typeValues, string idCategoria, string dateFilter)
        {
            var gResponse = new GenericResponse<List<VMProductsWeek>>();

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

                var ProductListWeek = new List<VMProductsWeek>();
                var prods = await _productService.List();
                int i = 0;
                var dateActual = TimeHelper.GetArgentinaTime();
                if (dateFilter != null)
                {
                    var dateSplit = dateFilter.Split('/');
                    dateActual = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]), 0, 0, 0);
                }

                foreach (KeyValuePair<string, string?> item in await _dashboardService.ProductsTopByCategory(typeValues, idCategoria, tiendaId, dateActual))
                {
                    var prod = prods.FirstOrDefault(_ => _.Description == item.Key);
                    if (prod != null)
                    {
                        ProductListWeek.Add(new VMProductsWeek()
                        {
                            Product = $"{++i}. {item.Key} ",
                            Quantity = $" {item.Value} {(prod.TipoVenta == Model.Enum.TipoVenta.U ? "U." : prod.TipoVenta)}"
                        });
                    }

                }

                gResponse.State = true;
                gResponse.Object = ProductListWeek;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar el top ventas de dashboard";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. TypeValues: {TypeValues}. DateFilter: {DateFilter}. IdCategoria: {IdCategoria}", errorMessage, typeValues.ToJson(), dateFilter.ToJson(), idCategoria.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al recuperar roles";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al recuperar un usuario";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idUser.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] IFormFile photo, [FromForm] string model)
        {

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                VMUser vmUser = JsonConvert.DeserializeObject<VMUser>(model);
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
                var errorMessage = "Error al crear usuarios";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] IFormFile photo, [FromForm] string model)
        {

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                VMUser vmUser = JsonConvert.DeserializeObject<VMUser>(model);
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
                var errorMessage = "Error al actualizar usuarios";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al borrar usuarios";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, IdUser.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public IActionResult TipoVenta()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recupero las formas de pago para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTipoVenta()
        {

            List<VMTypeDocumentSale> listUsers = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        //[HttpGet]
        //public async Task<IActionResult> GetTipoVentaWeb()
        //{
        //    List<VMTypeDocumentSale> listUsers = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.ListWeb());
        //    return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        //}


        [HttpPost]
        public async Task<IActionResult> CreateTipoVenta([FromBody] VMTypeDocumentSale model)
        {
            var gResponse = new GenericResponse<VMTypeDocumentSale>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var usuario_creado = await _typeDocumentSaleService.Add(_mapper.Map<TypeDocumentSale>(model));

                gResponse.Object = _mapper.Map<VMTypeDocumentSale>(usuario_creado);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al crear forma de pago";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateTipoVenta([FromBody] VMTypeDocumentSale model)
        {

            GenericResponse<VMTypeDocumentSale> gResponse = new GenericResponse<VMTypeDocumentSale>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                TypeDocumentSale user_edited = await _typeDocumentSaleService.Edit(_mapper.Map<TypeDocumentSale>(model));

                gResponse.Object = _mapper.Map<VMTypeDocumentSale>(user_edited);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar forma de pago";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTipoVenta(int idTypeDocumentSale)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                gResponse.State = await _typeDocumentSaleService.Delete(idTypeDocumentSale);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al eliminar forma de pago";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idTypeDocumentSale.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        public IActionResult Cliente()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
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
                var errorMessage = "Error al crear clientes";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al recuperar movimientos de clientes";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al crear cliente";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al actualizar cliente";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al eliminar cliente";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idCliente.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public IActionResult Proveedor()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recupera proveedores para DataTable y Selects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProveedores()
        {
            var gResponse = new GenericResponse<List<VMProveedor>>();
            try
            {
                var listProveedor = _mapper.Map<List<VMProveedor>>(await _proveedorService.List());
                return StatusCode(StatusCodes.Status200OK, new { data = listProveedor });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar proveedores";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedoresConProductos()
        {

            var gResponse = new GenericResponse<List<VMPedidosProveedor>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var listProveedor = await _proveedorService.ListConProductos(user.IdTienda);

                var list = listProveedor
                    .Select(p => new Proveedor
                    {
                        IdProveedor = p.IdProveedor,
                        Nombre = p.Nombre,
                        Products = p.Products.Select(prod => new Product
                        {
                            IdProduct = prod.IdProduct,
                            Description = prod.Description,
                            CostPrice = prod.CostPrice,
                            Stocks = prod.Stocks
                                        .Where(s => s.IdTienda == user.IdTienda)
                                        .Select(stock => new Stock
                                        {
                                            IdStock = stock.IdStock,
                                            StockActual = stock.StockActual
                                        }).ToList()
                        }).ToList()
                    })
                    .OrderBy(p => p.Nombre)
                    .ToList();

                gResponse.State = true;
                gResponse.Object = _mapper.Map<List<VMPedidosProveedor>>(list);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar proveedores con productos para pedido";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateProveedor([FromBody] VMProveedor model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var gResponse = new GenericResponse<VMProveedor>();
            try
            {
                var usuario_creado = await _proveedorService.Add(_mapper.Map<Proveedor>(model));

                gResponse.Object = _mapper.Map<VMProveedor>(usuario_creado);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al crear proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPagoProveedor([FromBody] VMProveedorMovimiento model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var gResponse = new GenericResponse<VMProveedorMovimiento>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.RegistrationUser = user.UserName;
                model.RegistrationDate = TimeHelper.GetArgentinaTime();
                model.idTienda = user.IdTienda;
                var usuario_creado = await _proveedorService.Add(_mapper.Map<ProveedorMovimiento>(model));

                model = _mapper.Map<VMProveedorMovimiento>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al crear pago a proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        /// <summary>
        /// Recupera movimientos de proveedor para DataTable
        /// </summary>
        /// <param name="idProveedor"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMovimientoProveedor(int idProveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(idProveedor);
            }
            var gResponse = new GenericResponse<List<VMProveedorMovimiento>>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var listUsers = _mapper.Map<List<VMProveedorMovimiento>>(await _proveedorService.ListMovimientosProveedor(idProveedor, user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar movimientos de proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idProveedor.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        /// <summary>
        /// Recupera movimientos de proveedores para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllMovimientoProveedor()
        {

            var gResponse = new GenericResponse<List<VMProveedorMovimiento>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var listUsers = _mapper.Map<List<VMProveedorMovimiento>>(await _proveedorService.ListMovimientosProveedorForTablaDinamica(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar pagos a proveedores";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedorTablaDinamica()
        {

            var gResponse = new GenericResponse<List<VMMovimientoProveedoresTablaDinamica>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var listUsers = _mapper.Map<List<VMMovimientoProveedoresTablaDinamica>>(await _proveedorService.ListMovimientosProveedorForTablaDinamica(user.IdTienda));
                gResponse.State = true;
                gResponse.Object = listUsers;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar pagos a proveedores para tabla dinamica";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProveedor([FromBody] VMProveedor vmUser)
        {
            if (!ModelState.IsValid)
            {
                return View(vmUser);
            }


            var gResponse = new GenericResponse<VMProveedor>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                vmUser.ModificationUser = user.UserName;
                var user_edited = await _proveedorService.Edit(_mapper.Map<Proveedor>(vmUser));

                vmUser = _mapper.Map<VMProveedor>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, vmUser.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProveedor(int idProveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(idProveedor);
            }


            var gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _proveedorService.Delete(idProveedor);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al eliminar proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idProveedor.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }


        [HttpPut]
        public async Task<IActionResult> CambioEstadoPagoProveedor(int idMovimiento)
        {
            if (!ModelState.IsValid)
            {
                return View(idMovimiento);
            }


            var gResponse = new GenericResponse<VMProveedor>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                _ = await _proveedorService.CambiarEstadoMovimiento(idMovimiento);

                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al cambiar estrado de pago de pago de proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idMovimiento.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdatePagoProveedor([FromBody] VMProveedorMovimiento vmUser)
        {
            if (!ModelState.IsValid)
            {
                return View(vmUser);
            }

            var gResponse = new GenericResponse<VMProveedorMovimiento>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                vmUser.ModificationUser = user.UserName;
                var user_edited = await _proveedorService.Edit(_mapper.Map<ProveedorMovimiento>(vmUser));

                vmUser = _mapper.Map<VMProveedorMovimiento>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar pago de proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, vmUser.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeletePagoProveedor(int idPagoProveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(idPagoProveedor);
            }

            var gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _proveedorService.DeleteProveedorMovimiento(idPagoProveedor);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al eliminar pago de proveedor";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idPagoProveedor.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public IActionResult Promociones()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recupera promociones para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPromociones()
        {
            var gResponse = new GenericResponse<List<VMPromocion>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.List(user.IdTienda));
                foreach (var p in listPromocion)
                {
                    await SetStringPromocion(p);

                }
                return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar promociones";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        private async Task SetStringPromocion(VMPromocion p)
        {
            var dias = string.Empty;
            var producto = string.Empty;
            var categoria = string.Empty;

            if (p.IdProducto != null)
            {
                var prod = await _productService.Get(Convert.ToInt32(p.IdProducto));
                p.PromocionString += " [" + string.Join(", ", prod.Description) + "]";
            }

            if (p.IdCategory != null && p.IdCategory.Any())
            {
                var catList = await _categoryService.GetMultiple(p.IdCategory);
                p.PromocionString += " [" + string.Join(", ", catList.Select(_ => _.Description)) + "]";
            }

            if (p.Dias != null && p.Dias.Any())
            {
                var diasList = p.Dias.Select(_ => (Model.Enum.DiasSemana)_).ToList();
                p.PromocionString += " [" + string.Join(", ", diasList.Select(_ => _.ToString())) + "]";
            }
            p.PromocionString += " -> ";
            p.PromocionString += p.Precio != null ? $"Precio fijo: ${p.Precio}" : $"Precio {p.Porcentaje}%: ";
        }

        [HttpGet]
        public async Task<IActionResult> GetPromocionesActivas()
        {
            var gResponse = new GenericResponse<List<VMPromocion>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.Activas(user.IdTienda));

                gResponse.State = true;
                gResponse.Object = listPromocion;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar promociones activas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromociones([FromBody] VMPromocion model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.IdTienda = user.IdTienda;
                var usuario_creado = await _promocionService.Add(_mapper.Map<Promocion>(model));

                model = _mapper.Map<VMPromocion>(usuario_creado);

                await SetStringPromocion(model);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al crear promocion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdatePromociones([FromBody] VMPromocion vmUser)
        {
            if (!ModelState.IsValid)
            {
                return View(vmUser);
            }


            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var user_edited = await _promocionService.Edit(_mapper.Map<Promocion>(vmUser));

                vmUser = _mapper.Map<VMPromocion>(user_edited);

                await SetStringPromocion(vmUser);
                gResponse.State = true;
                gResponse.Object = vmUser;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar promocion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, vmUser.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeletePromociones(int idPromocion)
        {
            if (!ModelState.IsValid)
            {
                return View(idPromocion);
            }


            var gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _promocionService.Delete(idPromocion);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al eliminar promocion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idPromocion.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }


        [HttpPut]
        public async Task<IActionResult> CambiarEstadoPromocion(int idPromocion)
        {
            if (!ModelState.IsValid)
            {
                return View(idPromocion);
            }


            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                var resp = ValidarAutorizacion([Roles.Administrador]);

                var user_edited = await _promocionService.CambiarEstado(idPromocion, resp.UserName);

                var model = _mapper.Map<VMPromocion>(user_edited);
                await SetStringPromocion(model);

                gResponse.Object = model;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al cambiar estado de promocion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, idPromocion.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Ajuste()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetAjuste()
        {
            var gResponse = new GenericResponse<VMAjustes>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmAjusteWeb = _mapper.Map<VMAjustes>(await _ajusteService.GetAjustesWeb());

                var vmAjuste = _mapper.Map<VMAjustes>(await _ajusteService.GetAjustes(user.IdTienda));

                vmAjusteWeb.CodigoSeguridad = vmAjuste.CodigoSeguridad;
                vmAjusteWeb.NombreImpresora = vmAjuste.NombreImpresora;
                vmAjusteWeb.MinimoIdentificarConsumidor = vmAjuste.MinimoIdentificarConsumidor;
                vmAjusteWeb.ImprimirDefault = vmAjuste.ImprimirDefault;
                vmAjusteWeb.NombreTiendaTicket = vmAjuste.NombreTiendaTicket;
                vmAjusteWeb.ControlStock = vmAjuste.ControlStock;
                vmAjusteWeb.IdTienda = vmAjuste.IdTienda;

                gResponse.Object = vmAjusteWeb;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar ajuste web";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateAjuste([FromBody] VMAjustes model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            GenericResponse<VMAjustes> gResponse = new GenericResponse<VMAjustes>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                model.ModificationUser = user.UserName;
                model.IdTienda = user.IdTienda;
                var edited_AjusteWeb = await _ajusteService.EditWeb(_mapper.Map<AjustesWeb>(model));

                var edited_Ajuste = await _ajusteService.Edit(_mapper.Map<Ajustes>(model));

                model = _mapper.Map<VMAjustes>(edited_AjusteWeb);
                model.CodigoSeguridad = edited_Ajuste.CodigoSeguridad;
                model.NombreImpresora = edited_Ajuste.NombreImpresora;
                model.MinimoIdentificarConsumidor = edited_Ajuste.MinimoIdentificarConsumidor;
                model.ImprimirDefault = edited_Ajuste.ImprimirDefault;
                model.NombreTiendaTicket = edited_Ajuste.NombreTiendaTicket;
                model.ControlStock = edited_Ajuste.ControlStock;
                model.IdTienda = edited_Ajuste.IdTienda;

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar ajuste";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {Model}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        /// <summary>
        /// Recupera ajustes para saber aumento web
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAjustesProductos()
        {
            var gResponse = new GenericResponse<string>();
            try
            {
                var ajuste = await _ajusteService.GetAjustesWeb();

                gResponse.State = true;
                gResponse.Object = ajuste.AumentoWeb.HasValue ? ajuste.AumentoWeb.Value.ToString("F0") : string.Empty;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar aumento web";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        /// <summary>
        /// Recupera ajustes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAjustesVentas()
        {
            var gResponse = new GenericResponse<VMAjustes?>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);

                var ajuste = await _ajusteService.GetAjustes(user.IdTienda);
                var vmAjuste = _mapper.Map<VMAjustes>(ajuste);

                gResponse.State = true;
                gResponse.Object = vmAjuste;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar nombre de impresora";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateSecurityCode(string encryptedCode)
        {
            if (!ModelState.IsValid)
            {
                return View(encryptedCode);
            }
            var gResponse = new GenericResponse<bool>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);

                var ajuste = await _ajusteService.GetAjustes(user.IdTienda);

                var codigo = ajuste.CodigoSeguridad != null ? ajuste.CodigoSeguridad : string.Empty;

                gResponse.State = true;
                gResponse.Object = encryptedCode == codigo;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al validar codigo de seguridad";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Facturacion()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Retorna las facturas para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFacturas()
        {
            var gResponse = new GenericResponse<List<VMFacturaEmitida>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var facturas = await _afipService.GetAll(user.IdTienda);
                var vmFactura = _mapper.Map<List<VMFacturaEmitida>>(facturas);

                return StatusCode(StatusCodes.Status200OK, new { data = vmFactura });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar facturas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetFactura(int idFacturaEmitida)
        {
            var gResponse = new GenericResponse<VMFacturaEmitida>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var factura = await _afipService.GetById(idFacturaEmitida);

                var vmFactura = _mapper.Map<VMFacturaEmitida>(factura);

                if (factura.Errores == null)
                {
                    vmFactura.QR = await _afipService.GenerateFacturaQR(factura);
                }

                gResponse.State = true;
                gResponse.Object = vmFactura;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar facturas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAjustesFacturacion()
        {
            var gResponse = new GenericResponse<VMAjustesFacturacion>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmAjusteWeb = _mapper.Map<VMAjustesFacturacion>(await _ajusteService.GetAjustesFacturacion(user.IdTienda));

                gResponse.Object = vmAjusteWeb;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar ajuste de facturacion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetValidateCertificate()
        {
            var gResponse = new GenericResponse<string>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);
                var ajustes = await _ajusteService.GetAjustesFacturacion(user.IdTienda);
                var resp = _afipService.ValidateCertificate(ajustes);

                gResponse.Object = resp;
                gResponse.State = resp == string.Empty;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error en el certificado", _logger);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAjustesFacturacion([FromForm] IFormFile Certificado, [FromForm] string model)
        {

            GenericResponse<VMAjustesFacturacion> gResponse = new GenericResponse<VMAjustesFacturacion>();
            try
            {
                var vmModel = JsonConvert.DeserializeObject<VMAjustesFacturacion>(model);
                var user = ValidarAutorizacion([Roles.Administrador]);

                if (Certificado != null)
                {
                    var oldAjustes = await _ajusteService.GetAjustesFacturacion(user.IdTienda);

                    var pathFile = await _afipService.ReplaceCertificateAsync(Certificado, user.IdTienda, oldAjustes.CertificadoNombre);

                    vmModel.CertificadoNombre = Certificado.FileName;
                    if (!string.IsNullOrEmpty(vmModel.CertificadoPassword))
                    {
                        var cert = _afipService.GetCertificateAfipInformation(pathFile, vmModel.CertificadoPassword);
                        if (cert != null)
                        {
                            vmModel.CertificadoFechaCaducidad = cert.FechaCaducidad;
                            vmModel.CertificadoFechaInicio = cert.FechaInicio;
                            vmModel.Cuit = Convert.ToInt64(cert.Cuil);
                        }
                    }
                }

                vmModel.ModificationUser = user.UserName;
                vmModel.IdTienda = user.IdTienda;
                var ajustes = await _ajusteService.EditFacturacion(_mapper.Map<AjustesFacturacion>(vmModel));

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMAjustesFacturacion>(ajustes);
                gResponse.Message = string.Empty;

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar ajustes de facturación", _logger, model);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
