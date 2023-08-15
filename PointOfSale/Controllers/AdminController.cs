using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Globalization;
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

        public AdminController(
            IDashBoardService dashboardService,
            IUserService userService,
            IRolService rolService,
            ITypeDocumentSaleService typeDocumentSaleService,
            IClienteService clienteService,
            IProveedorService proveedorService,
            IMapper mapper,
            IPromocionService promocionService)
        {
            _dashboardService = dashboardService;
            _userService = userService;
            _rolService = rolService;
            _mapper = mapper;
            _typeDocumentSaleService = typeDocumentSaleService;
            _clienteService = clienteService;
            _proveedorService = proveedorService;
            _promocionService = promocionService;


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
        public async Task<IActionResult> GetSummary(TypeValuesDashboard typeValues)
        {
            var vmDashboard = new VMDashBoard();

            var ejeXint = new int[0];
            var ejeX = new string[0];
            var dateCompare = DateTime.Now;

            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:

                    vmDashboard.Actual = "Hoy";
                    vmDashboard.Anterior = "Ayer";
                    vmDashboard.EjeXLeyenda = "Horas";
                    break;
                case TypeValuesDashboard.Semana:
                    ejeXint = new int[7];
                    ejeX = new string[7];
                    dateCompare = DateTime.Now.AddDays(-(6 + (int)DateTime.Now.DayOfWeek));

                    for (int i = 0; i < 7; i++)
                    {
                        ejeXint[i] = i;
                        ejeX[i] = ((DiasSemana)i + 1).ToString();
                    }

                    vmDashboard.Actual = "Semana actual";
                    vmDashboard.Anterior = "Semana pasada";
                    vmDashboard.EjeXLeyenda = "Dias";

                    break;
                case TypeValuesDashboard.Mes:
                    var cantDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month);
                    ejeXint = new int[cantDaysInMonth];
                    ejeX = new string[cantDaysInMonth];
                    dateCompare = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                    for (int i = 0; i < cantDaysInMonth; i++)
                    {
                        ejeXint[i] = i;
                        ejeX[i] = (i + 1).ToString();
                    }
                    vmDashboard.Actual = "Mes actual";
                    vmDashboard.Anterior = "Mes pasado";
                    vmDashboard.EjeXLeyenda = "Dias";
                    break;
            }


            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                List<VMSalesWeek> listSales = new List<VMSalesWeek>();
                List<VMSalesWeek> listSalesComparacion = new List<VMSalesWeek>();


                int i = 0;
                var resultados = await _dashboardService.GetSales(typeValues);


                switch (typeValues)
                {
                    case TypeValuesDashboard.Dia:
                        var first = resultados.VentasComparacionHour.FirstOrDefault().Key;
                        var last = resultados.VentasComparacionHour.Last().Key;
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
                        foreach (KeyValuePair<DateTime, decimal> item in resultados.VentasActuales)
                        {
                            while ((int)item.Key.Date.DayOfWeek > (int)dateCompare.AddDays(ejeXint[i]).DayOfWeek)
                            {
                                listSales.Add(new VMSalesWeek() { Total = 0m });
                                i++;
                            }

                            listSales.Add(new VMSalesWeek() { Total = item.Value });

                            i++;
                        }
                        listSalesComparacion.AddRange(GetSalesComparacion(ejeXint, dateCompare, resultados));

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
                        listSalesComparacion.AddRange(GetSalesComparacion(ejeXint, dateCompare, resultados));

                        break;
                }

                var ProductListWeek = new List<VMProductsWeek>();

                foreach (KeyValuePair<string, int> item in await _dashboardService.ProductsTop(typeValues))
                {
                    ProductListWeek.Add(new VMProductsWeek()
                    {
                        Product = item.Key,
                        Quantity = item.Value
                    });
                }

                vmDashboard.EjeX = ejeX;
                vmDashboard.SalesList = listSales.Select(_ => _.Total).ToList();
                vmDashboard.SalesListComparacion = listSalesComparacion.Select(_ => _.Total).ToList();


                vmDashboard.TotalSales = "$ " + listSales.Sum(_ => _.Total);
                vmDashboard.TotalSalesComparacion = "$ " + listSalesComparacion.Sum(_ => _.Total);

                vmDashboard.ProductsTopLastWeek = ProductListWeek;
                //vmDashboard.TotalIncome = "$ " + await _dashboardService.TotalIncomeLastWeek();
                //vmDashboard.TotalProducts = await _dashboardService.TotalProducts();
                //vmDashboard.TotalCategories = await _dashboardService.TotalCategories();

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();

                foreach (KeyValuePair<string, decimal> item in await _dashboardService.GetSalesByTypoVenta(typeValues))
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }
                vmDashboard.VentasPorTipoVenta = VentasPorTipoVenta;

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

        private static List<VMSalesWeek> GetSalesComparacion(int[] ejeXint, DateTime dateCompare, GraficoVentasConComparacion resultados)
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
            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();
            try
            {
                VMUser vmUser = JsonConvert.DeserializeObject<VMUser>(model);

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
        public async Task<IActionResult> GetCliente(int idCliente)
        {
            var listUsers = _mapper.Map<List<VMCliente>>(await _clienteService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpGet]
        public async Task<IActionResult> GetMovimientoCliente(int idCliente)
        {
            var listUsers = _mapper.Map<List<VMClienteMovimiento>>(await _clienteService.ListMovimientoscliente(idCliente));
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCliente([FromBody] VMCliente model)
        {
            var gResponse = new GenericResponse<VMCliente>();
            try
            {
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
        public async Task<IActionResult> GetProveedor()
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


        public IActionResult Promociones()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPromociones()
        {
            var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });
        }

        [HttpGet]
        public async Task<IActionResult> GetPromocionesActivas()
        {
            var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.Activas());
            return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromociones([FromBody] VMPromocion model)
        {
            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
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
    }
}
