using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static iTextSharp.text.pdf.events.IndexEvents;
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
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
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
                var errorMessage = "Error al recuperar lista de pedidos";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al crear pedido";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage} Request: {RequestModel}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al actualizar pedido";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage} Request: {RequestModel}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al elimiar pedido";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage} Request: {RequestModel}.", errorMessage, idPedido.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al cerrar pedido";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage} Request: {RequestModel}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }
    }
}
