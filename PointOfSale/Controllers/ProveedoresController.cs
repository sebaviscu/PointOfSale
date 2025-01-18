using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class ProveedoresController : BaseController
    {
        private readonly ILogger<ProveedoresController> _logger;
        private readonly IMapper _mapper;
        private readonly IProveedorService _proveedorService;

        public ProveedoresController(IMapper mapper, ILogger<ProveedoresController> logger,IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
        }

        /// <summary>
        /// Recupera proveedores para DataTable y Selects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProveedores(bool visionGlobal)
        {
            var gResponse = new GenericResponse<List<VMProveedor>>();
            try
            {
                var listProveedor = _mapper.Map<List<VMProveedor>>(await _proveedorService.List());
                return StatusCode(StatusCodes.Status200OK, new { data = listProveedor });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar proveedores", _logger);
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
                return HandleException(ex, "Error al recuperar proveedores con productos para pedido", _logger);
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
                return HandleException(ex, "Error al crear proveedor", _logger, model);
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
                return HandleException(ex, "Error al crear pago a proveedor", _logger, model);
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
                return HandleException(ex, "Error al recuperar movimientos de proveedor", _logger, idProveedor);
            }

        }

        /// <summary>
        /// Recupera movimientos de proveedores para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllMovimientoProveedor(bool visionGlobal)
        {

            var gResponse = new GenericResponse<List<VMProveedorMovimiento>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var idtienda = visionGlobal ? 0 : user.IdTienda;

                var listUsers = _mapper.Map<List<VMProveedorMovimiento>>(await _proveedorService.ListMovimientosProveedorForTablaDinamica(idtienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar pagos a proveedores", _logger);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedorTablaDinamica(bool visionGlobal)
        {

            var gResponse = new GenericResponse<List<VMMovimientoProveedoresTablaDinamica>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var idtienda = visionGlobal ? 0 : user.IdTienda;

                var listUsers = _mapper.Map<List<VMMovimientoProveedoresTablaDinamica>>(await _proveedorService.ListMovimientosProveedorForTablaDinamica(idtienda));
                gResponse.State = true;
                gResponse.Object = listUsers;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar pagos a proveedores para tabla dinamica", _logger);
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
                return HandleException(ex, "Error al actualizar proveedor", _logger, vmUser);
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
                return HandleException(ex, "Error al eliminar proveedor", _logger, idProveedor);
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
                return HandleException(ex, "Error al cambiar estrado de pago de pago de proveedor", _logger, idMovimiento.ToJson());
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
                return HandleException(ex, "Error al actualizar pago de proveedor", _logger, vmUser);
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
                return HandleException(ex, "Error al eliminar pago de proveedor", _logger, idPagoProveedor);
            }

        }
    }
}
