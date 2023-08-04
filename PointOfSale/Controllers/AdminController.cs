using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
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
        public async Task<IActionResult> GetSummary()
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();

            try
            {
                VMDashBoard vmDashboard = new VMDashBoard();

                vmDashboard.TotalSales = await _dashboardService.TotalSalesLastWeek();
                vmDashboard.TotalIncome = "$ " + await _dashboardService.TotalIncomeLastWeek();
                vmDashboard.TotalProducts = await _dashboardService.TotalProducts();
                vmDashboard.TotalCategories = await _dashboardService.TotalCategories();

                List<VMSalesWeek> listSalesWeek = new List<VMSalesWeek>();
                List<VMProductsWeek> ProductListWeek = new List<VMProductsWeek>();

                foreach (KeyValuePair<string, int> item in await _dashboardService.SalesLastWeek())
                {
                    listSalesWeek.Add(new VMSalesWeek()
                    {
                        Date = item.Key,
                        Total = item.Value
                    });
                }
                foreach (KeyValuePair<string, int> item in await _dashboardService.ProductsTopLastWeek())
                {
                    ProductListWeek.Add(new VMProductsWeek()
                    {
                        Product = item.Key,
                        Quantity = item.Value
                    });
                }

                vmDashboard.SalesLastWeek = listSalesWeek;
                vmDashboard.ProductsTopLastWeek = ProductListWeek;

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
            try
            {
            var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });

            }
            catch (Exception e)
            {

                throw;
            }
            return default;
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
