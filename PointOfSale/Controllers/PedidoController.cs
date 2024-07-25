using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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

        public PedidoController(IMapper mapper, IPedidoService pedidoService)
        {
            _mapper = mapper;
            _pedidoService = pedidoService;
        }

        public IActionResult Pedido()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetPedidos()
        {
            try
            {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });
                var list = await _pedidoService.List(user.IdTienda);

            List<VMPedido> vmPedidoList = _mapper.Map<List<VMPedido>>(list);
            return StatusCode(StatusCodes.Status200OK, new { data = vmPedidoList.OrderBy(_ => _.Orden).ThenByDescending(_=>_.IdPedido).ToList() });
            }
            catch (Exception e)
            {

                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreatePedido([FromBody] VMPedido model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMPedido> gResponse = new GenericResponse<VMPedido>();
            try
            {
                model.RegistrationUser = user.UserName;
                model.IdTienda = user.IdTienda;

                Pedido pedido_created = await _pedidoService.Add(_mapper.Map<Pedido>(model));

                model = _mapper.Map<VMPedido>(pedido_created);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.ToString();
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);

        }

        [HttpPut]
        public async Task<IActionResult> UpdatePedidos([FromBody] VMPedido model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMPedido> gResponse = new GenericResponse<VMPedido>();
            try
            {
                model.UsuarioFechaCerrado = user.UserName;

                Pedido edited_Pedido = await _pedidoService.Edit(_mapper.Map<Pedido>(model));

                model = _mapper.Map<VMPedido>(edited_Pedido);

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

        [HttpDelete]
        public async Task<IActionResult> DeletePedido(int idPedido)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _pedidoService.Delete(idPedido);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpPut]
        public async Task<IActionResult> CerrarPedidos([FromBody] VMPedido model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMPedido> gResponse = new GenericResponse<VMPedido>();
            try
            {
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
