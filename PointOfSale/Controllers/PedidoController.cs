using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class PedidoController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidoController> _logger;

        public PedidoController(IMapper mapper, IPedidoService pedidoService, ILogger<PedidoController> logger)
        {
            _mapper = mapper;
            _pedidoService = pedidoService;
            _logger = logger;
        }

        public IActionResult Pedido()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador, Roles.Encargado]);
        }

        [HttpGet]
        public async Task<IActionResult> ViewById(int idPedido)
        {
            ValidarAutorizacion([Roles.Administrador]);

            if (!HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Access");

            ViewData["IdPedido"] = idPedido;

            return View("Pedido");
        }

        [HttpGet]
        public async Task<IActionResult> GetPedidos()
        {
            var gResponse = new GenericResponse<List<VMPedido>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
                var list = await _pedidoService.List(user.IdTienda);

                List<VMPedido> vmPedidoList = _mapper.Map<List<VMPedido>>(list);
                return StatusCode(StatusCodes.Status200OK, new { data = vmPedidoList.OrderBy(_ => _.Orden).ThenByDescending(_ => _.IdPedido).ToList() });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de pedidos.", _logger);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetPedidoByProveedor(int idProveedor)
        {
            var gResponse = new GenericResponse<List<VMPedido>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
                var list = await _pedidoService.GetByProveedor(user.IdTienda, idProveedor);

                List<VMPedido> vmPedidoList = _mapper.Map<List<VMPedido>>(list);
                return StatusCode(StatusCodes.Status200OK, new { data = vmPedidoList.OrderBy(_ => _.Orden).ThenByDescending(_ => _.IdPedido).ToList() });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de pedidos.", _logger);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreatePedido([FromBody] VMPedido model)
        {

            GenericResponse<VMPedido> gResponse = new GenericResponse<VMPedido>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.RegistrationUser = user.UserName;
                model.IdTienda = user.IdTienda;

                Pedido pedido_created = await _pedidoService.Add(_mapper.Map<Pedido>(model));

                model = _mapper.Map<VMPedido>(pedido_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear pedido.", _logger, model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePedidos([FromBody] VMPedido model)
        {

            GenericResponse<VMPedido> gResponse = new GenericResponse<VMPedido>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                model.UsuarioFechaCerrado = user.UserName;

                Pedido edited_Pedido = await _pedidoService.Edit(_mapper.Map<Pedido>(model));

                model = _mapper.Map<VMPedido>(edited_Pedido);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar pedido.", _logger, model);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeletePedido(int idPedido)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                gResponse.State = await _pedidoService.Delete(idPedido);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar pedido.", _logger, idPedido);
            }
        }


        [HttpPut]
        public async Task<IActionResult> CerrarPedidos([FromBody] VMPedido model)
        {

            GenericResponse<VMPedido> gResponse = new GenericResponse<VMPedido>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var m = new VMProveedorMovimiento();
                m.NroFactura = model.NroFactura;
                m.TipoFactura = model.TipoFactura;
                m.EstadoPago = model.EstadoPago;
                m.IdProveedor = model.IdProveedor;
                m.Importe = model.ImporteEstimado.Value;
                m.ImporteSinIva = model.ImporteSinIva;
                m.Iva = model.Iva;
                m.IvaImporte = model.IvaImporte;
                m.RegistrationDate = TimeHelper.GetArgentinaTime();
                m.RegistrationUser = user.UserName;
                m.idTienda = user.IdTienda;
                m.IdPedido = model.IdPedido;
                m.Comentario = model.Comentario;
                m.FacturaPendiente = model.FacturaPendiente;

                model.ProveedorMovimiento = m;
                model.UsuarioFechaCerrado = user.UserName;

                Pedido edited_Pedido = await _pedidoService.CerrarPedido(_mapper.Map<Pedido>(model));

                model = _mapper.Map<VMPedido>(edited_Pedido);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cerrar pedido.", _logger, model);
            }

        }
    }
}
