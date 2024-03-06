using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Model.Auditoria;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Globalization;
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

        public AdminController(
            IDashBoardService dashboardService,
            IUserService userService,
            IRolService rolService,
            ITypeDocumentSaleService typeDocumentSaleService,
            IClienteService clienteService,
            IProveedorService proveedorService,
            IMapper mapper,
            IPromocionService promocionService, IProductService productService, ICategoryService categoryService)
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
        }

        public IActionResult DashBoard()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(TypeValuesDashboard typeValues, string dateFilter)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var vmDashboard = new VMDashBoard();

            var ejeXint = new int[0];
            var ejeX = new string[0];
            var dateActual = SetDate(typeValues, dateFilter);
            DateTime dateCompare;
            var textoFiltroDiaSemanaMes = string.Empty;

            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:
                    dateCompare = dateActual;
                    vmDashboard.Actual = "Hoy";
                    vmDashboard.Anterior = "Ayer";
                    vmDashboard.EjeXLeyenda = "Horas";
                    textoFiltroDiaSemanaMes = dateActual.Date.ToShortDateString();
                    break;

                case TypeValuesDashboard.Semana:
                    ejeXint = new int[7];
                    ejeX = new string[7];
                    dateCompare = dateActual.AddDays(-(6 + (int)dateActual.DayOfWeek));

                    for (int i = 0; i < 7; i++)
                    {
                        ejeXint[i] = i + 1;
                        ejeX[i] = ((DiasSemana)i + 1).ToString();
                    }

                    vmDashboard.Actual = "Semana actual";
                    vmDashboard.Anterior = "Semana pasada";
                    vmDashboard.EjeXLeyenda = "Dias";

                    var weekInt = (int)dateActual.DayOfWeek != 0 ? (int)dateActual.DayOfWeek : 7; // si es domingo, hay problemas

                    var fechaString = dateActual.AddDays(-(weekInt - 1));

                    textoFiltroDiaSemanaMes = $"{fechaString.ToShortDateString()} - {fechaString.AddDays(6).ToShortDateString()}";
                    break;

                case TypeValuesDashboard.Mes:
                    var cantDaysInMonth = DateTime.DaysInMonth(dateActual.Date.Year, dateActual.Date.Month);
                    ejeXint = new int[cantDaysInMonth];
                    ejeX = new string[cantDaysInMonth];
                    dateCompare = new DateTime(dateActual.Year, dateActual.Month, 1);

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


            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                List<VMSalesWeek> listSales = new List<VMSalesWeek>();
                List<VMSalesWeek> listSalesComparacion = new List<VMSalesWeek>();


                int i = 0;
                var resultados = await _dashboardService.GetSales(typeValues, user.IdTienda, dateActual);


                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        var firstComp = resultados.VentasComparacionHour.FirstOrDefault().Key;
                        var firstVenta = resultados.VentasActualesHour.Count() > 0 ? resultados.VentasActualesHour.FirstOrDefault().Key : firstComp;
                        if (firstComp == 0) firstComp = firstVenta;
                        if (firstVenta == 0) firstVenta = firstComp;

                        var first = Math.Min(firstComp, firstVenta);

                        var lastComp = resultados.VentasComparacionHour.LastOrDefault().Key;
                        var lastVenta = resultados.VentasActualesHour.Count() > 0 ? resultados.VentasActualesHour.LastOrDefault().Key : lastComp;
                        if (lastComp == 0) lastComp = lastVenta;
                        if (lastVenta == 0) lastVenta = lastComp;

                        var last = Math.Max(lastComp, lastVenta);

                        if(dateFilter != null)
                        {
                            first = 0;
                            last = 23;
                        }

                        ejeXint = new int[(last - first) + 1];
                        ejeX = new string[(last - first) + 1];

                        for (i = 0; i < (last - first) + 1; i++)
                        {
                            ejeXint[i] = (i + first);
                            ejeX[i] = (i + first).ToString();
                        }
                        i = 0;

                        foreach (KeyValuePair<int, decimal> item in resultados.VentasActualesHour)
                        {
                            while ((int)item.Key > ejeXint[i])
                            {
                                listSales.Add(new VMSalesWeek() { Total = 0m });
                                i++;
                            }

                            listSales.Add(new VMSalesWeek() { Total = item.Value });

                            i++;
                        }

                        listSalesComparacion.AddRange(GetSalesComparacionHour(ejeXint, dateCompare, resultados));

                        break;
                    case TypeValuesDashboard.Semana:
                        listSales.AddRange(GetSalesComparacionWeek(ejeXint, dateCompare, resultados.VentasActuales, false));
                        listSalesComparacion.AddRange(GetSalesComparacionWeek(ejeXint, dateCompare, resultados.VentasComparacion, true));
                        
                        if(dateFilter != null)
                        {
                            while (listSales.Count != listSalesComparacion.Count)
                            {
                                listSales.Add(new VMSalesWeek() { Total = 0m });
                            }
                        }
                        break;
                    case TypeValuesDashboard.Mes:
                        foreach (KeyValuePair<DateTime, decimal> item in resultados.VentasActuales)
                        {
                            while (item.Key.Date > dateCompare.AddDays(ejeXint[i]))
                            {
                                listSales.Add(new VMSalesWeek() { Total = 0m });
                                i++;
                            }

                            listSales.Add(new VMSalesWeek() { Total = item.Value });

                            i++;
                        }
                        listSalesComparacion.AddRange(GetSalesComparacionMonth(ejeXint, dateCompare, resultados));

                        if(dateFilter != null)
                        {
                            while (listSales.Count != listSalesComparacion.Count)
                            {
                                listSales.Add(new VMSalesWeek() { Total = 0m });
                            }
                        }

                        break;
                }

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();

                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetSalesByTypoVenta(typeValues, user.IdTienda, dateActual))
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }

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

                var gananciaBruta = listSales.Sum(_ => _.Total);
                var gastosTotales = gastosProvTotales + totGastosParticualres + totGastosSueldos;

                vmDashboard.TextoFiltroDiaSemanaMes = textoFiltroDiaSemanaMes;

                vmDashboard.EjeX = ejeX;
                vmDashboard.SalesList = listSales.Select(_ => (int)_.Total).ToList();
                vmDashboard.SalesListComparacion = listSalesComparacion.Select(_ => (int)_.Total).ToList();
                vmDashboard.TotalSales = "$ " + listSales.Sum(_ => _.Total).ToString("F0");
                vmDashboard.TotalSalesComparacion = "$ " + listSalesComparacion.Sum(_ => _.Total).ToString("F0");
                vmDashboard.VentasPorTipoVenta = VentasPorTipoVenta;
                vmDashboard.GastosTotales = "$ " + gastosTotales.ToString("F0");
                vmDashboard.CantidadClientes = resultados.CantidadClientes;
                vmDashboard.Ganancia = "$ " + (gananciaBruta - gastosTotales).ToString("F0");

                vmDashboard.GastosTexto = $"${totGastosParticualres.ToString("F0")}";
                vmDashboard.GastosProvvedoresTexto = $"${gastosProvTotales.ToString("F0")}";
                vmDashboard.GastosSueldosTexto = $"$ {totGastosSueldos.ToString("F0")}";
                vmDashboard.GastosPorTipo = gastosParticualresList;
                vmDashboard.GastosPorTipoProveedor = gastosProveedores;
                vmDashboard.GastosPorTipoSueldos = gastosSueldosList;

                gResponse.State = true;
                gResponse.Object = vmDashboard;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public DateTime SetDate(TypeValuesDashboard typeValues, string dateFilter)
        {
            var dateActual = DateTime.Now;

            if (dateFilter != null)
            {
                var dateSplit = dateFilter.Split('/');
                dateActual = new DateTime(Convert.ToInt32(dateSplit[2]), Convert.ToInt32(dateSplit[1]), Convert.ToInt32(dateSplit[0]), 0, 0, 0);
                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        dateActual = dateActual.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                    case TypeValuesDashboard.Semana:
                        var weekNumber = (int)dateActual.DayOfWeek;
                        if (weekNumber < 7)
                        {
                            var diff = 7 - weekNumber;
                            dateActual = dateActual.AddDays(diff);
                        }
                        break;
                    case TypeValuesDashboard.Mes:
                        var monthNumber = dateActual.Day;
                        var daysInMonth = DateTime.DaysInMonth(dateActual.Year, dateActual.Month);

                        if (monthNumber < daysInMonth)
                        {
                            var diff = daysInMonth - monthNumber;
                            dateActual = dateActual.AddDays(diff);
                        }
                        break;
                    default:
                        break;
                }
            }

            return dateActual;
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesByTypoVenta(TypeValuesDashboard typeValues, string idCategoria)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });
            var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

            var ProductListWeek = new List<VMProductsWeek>();
            var prods = await _productService.List();
            int i = 0;
            var dateActual = DateTime.Now;
            foreach (KeyValuePair<string, string?> item in await _dashboardService.ProductsTopByCategory(typeValues, idCategoria, tiendaId, dateActual))
            {
                var prod = prods.FirstOrDefault(_ => _.Description == item.Key);
                ProductListWeek.Add(new VMProductsWeek()
                {
                    Product = $"{++i}. {item.Key} ",
                    Quantity = $" {item.Value} {(prod.TipoVenta == Model.Enum.TipoVenta.U ? "U." : prod.TipoVenta)}"
                });
            }

            return StatusCode(StatusCodes.Status200OK, ProductListWeek);
        }

        private static List<VMSalesWeek> GetSalesComparacionWeek(int[] ejeXint, DateTime dateCompare, Dictionary<DateTime, decimal> resultados, bool beforeWeek)
        {
            var lis = new List<VMSalesWeek>();
            var i = 0;
            foreach (KeyValuePair<DateTime, decimal> item in resultados)
            {
                var intDayOfWeek = (int)item.Key.Date.DayOfWeek == 0 ? 7 : (int)item.Key.Date.DayOfWeek + 1;
                while (intDayOfWeek > (i == 6 ? 8 : (int)dateCompare.AddDays(ejeXint[i]).DayOfWeek))
                {
                    lis.Add(new VMSalesWeek() { Total = 0m });
                    i++;
                }

                lis.Add(new VMSalesWeek() { Total = item.Value });

                i++;
            }

            while (beforeWeek && i < 7)
            {
                lis.Add(new VMSalesWeek() { Total = 0m });
                i++;
            }

            return lis;
        }

        private static List<VMSalesWeek> GetSalesComparacionMonth(int[] ejeXint, DateTime dateCompare, GraficoVentasConComparacion resultados)
        {
            var lis = new List<VMSalesWeek>();
            for (int x = 0; x < ejeXint.Length; x++)
            {

                var item = resultados.VentasComparacion.FirstOrDefault(_ => _.Key.Date.Day == dateCompare.AddDays(ejeXint[x]).Day);
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
            List<VMRol> listRoles = _mapper.Map<List<VMRol>>(await _rolService.List());
            return StatusCode(StatusCodes.Status200OK, listRoles);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<VMUser> listUsers = _mapper.Map<List<VMUser>>(await _userService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] IFormFile photo, [FromForm] string model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                VMUser vmUser = JsonConvert.DeserializeObject<VMUser>(model);
                vmUser.IdTienda = user.IdTienda;

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

                vmUser = _mapper.Map<VMUser>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] IFormFile photo, [FromForm] string model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                VMUser vmUser = JsonConvert.DeserializeObject<VMUser>(model);
                vmUser.ModificationUser = user.UserName;

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

                vmUser = _mapper.Map<VMUser>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int IdUser)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _userService.Delete(IdUser);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public IActionResult TipoVenta()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTipoVenta()
        {
            List<VMTypeDocumentSale> listUsers = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpGet]
        public async Task<IActionResult> GetTipoVentaWeb()
        {
            List<VMTypeDocumentSale> listUsers = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.ListWeb());
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }


        [HttpPost]
        public async Task<IActionResult> CreateTipoVenta([FromBody] VMTypeDocumentSale vmUser)
        {
            GenericResponse<VMTypeDocumentSale> gResponse = new GenericResponse<VMTypeDocumentSale>();
            try
            {
                TypeDocumentSale usuario_creado = await _typeDocumentSaleService.Add(_mapper.Map<TypeDocumentSale>(vmUser));

                vmUser = _mapper.Map<VMTypeDocumentSale>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTipoVenta([FromBody] VMTypeDocumentSale vmUser)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMTypeDocumentSale> gResponse = new GenericResponse<VMTypeDocumentSale>();
            try
            {
                //VMTypeDocumentSale vmUser = JsonConvert.DeserializeObject<VMTypeDocumentSale>(model);

                TypeDocumentSale user_edited = await _typeDocumentSaleService.Edit(_mapper.Map<TypeDocumentSale>(vmUser));

                vmUser = _mapper.Map<VMTypeDocumentSale>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTipoVenta(int idTypeDocumentSale)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _typeDocumentSaleService.Delete(idTypeDocumentSale);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        public IActionResult Cliente()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCliente()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var listUsers = _mapper.Map<List<VMCliente>>(await _clienteService.List(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpGet]
        public async Task<IActionResult> GetMovimientoCliente(int idCliente)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var listUsers = _mapper.Map<List<VMClienteMovimiento>>(await _clienteService.ListMovimientoscliente(idCliente, user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCliente([FromBody] VMCliente model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var gResponse = new GenericResponse<VMCliente>();
            try
            {
                model.IdTienda = user.IdTienda;
                var usuario_creado = await _clienteService.Add(_mapper.Map<Cliente>(model));

                model = _mapper.Map<VMCliente>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCliente([FromBody] VMCliente vmUser)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<VMCliente>();
            try
            {
                vmUser.ModificationUser = user.UserName;
                var user_edited = await _clienteService.Edit(_mapper.Map<Cliente>(vmUser));

                vmUser = _mapper.Map<VMCliente>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCliente(int idCliente)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _clienteService.Delete(idCliente);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public IActionResult Proveedor()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedores()
        {
            var listProveedor = _mapper.Map<List<VMProveedor>>(await _proveedorService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listProveedor });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProveedor([FromBody] VMProveedor model)
        {
            var gResponse = new GenericResponse<VMProveedor>();
            try
            {
                var usuario_creado = await _proveedorService.Add(_mapper.Map<Proveedor>(model));

                model = _mapper.Map<VMProveedor>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPagoProveedor([FromBody] VMProveedorMovimiento model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<VMProveedorMovimiento>();
            try
            {
                model.RegistrationUser = user.UserName;
                model.RegistrationDate = DateTime.Now;
                model.idTienda = user.IdTienda;
                var usuario_creado = await _proveedorService.Add(_mapper.Map<ProveedorMovimiento>(model));

                model = _mapper.Map<VMProveedorMovimiento>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetMovimientoProveedor(int idProveedor)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var listUsers = _mapper.Map<List<VMProveedorMovimiento>>(await _proveedorService.ListMovimientosProveedor(idProveedor, user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovimientoProveedor()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var listUsers = _mapper.Map<List<VMProveedorMovimiento>>(await _proveedorService.ListMovimientosProveedorForTablaDinamica(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedorTablaDinamica()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var listUsers = _mapper.Map<List<VMMovimientoProveedoresTablaDinamica>>(await _proveedorService.ListMovimientosProveedorForTablaDinamica(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProveedor([FromBody] VMProveedor vmUser)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<VMProveedor>();
            try
            {
                var user_edited = await _proveedorService.Edit(_mapper.Map<Proveedor>(vmUser));

                vmUser = _mapper.Map<VMProveedor>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProveedor(int idProveedor)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _proveedorService.Delete(idProveedor);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePagoProveedor(int idMovimiento)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var gResponse = new GenericResponse<VMProveedor>();
            try
            {
                var user_edited = await _proveedorService.CambiarEstadoMovimiento(idMovimiento);


                gResponse.State = true;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public IActionResult Promociones()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPromociones()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.List(user.IdTienda));
            foreach (var p in listPromocion)
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
            return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });
        }

        [HttpGet]
        public async Task<IActionResult> GetPromocionesActivas()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.Activas(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromociones([FromBody] VMPromocion model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                model.IdTienda = user.IdTienda;
                var usuario_creado = await _promocionService.Add(_mapper.Map<Promocion>(model));

                model = _mapper.Map<VMPromocion>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePromociones([FromBody] VMPromocion vmUser)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                var user_edited = await _promocionService.Edit(_mapper.Map<Promocion>(vmUser));

                vmUser = _mapper.Map<VMPromocion>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePromociones(int idPromocion)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _promocionService.Delete(idPromocion);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpPut]
        public async Task<IActionResult> CambiarEstadoPromocion(int idPromocion)
        {
            var resp = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                var user_edited = await _promocionService.CambiarEstado(idPromocion, resp.UserName);

                gResponse.Object = _mapper.Map<VMPromocion>(user_edited);
                gResponse.State = true;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
