using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;
using PointOfSale.Business.Utilities;

namespace PointOfSale.Controllers
{
    public class TurnoController : BaseController
    {
        private readonly ITurnoService _turnoService;
        private readonly IMapper _mapper;
        private readonly IDashBoardService _dashBoardService;
        private readonly ILogger<TurnoController> _logger;
        private readonly ITicketService _ticketService;
        private readonly IAjusteService _ajustesService;

        public TurnoController(ITurnoService turnoService,
            IMapper mapper,
            IDashBoardService dashBoardService,
            ILogger<TurnoController> logger,
            ITicketService ticketService,
            IAjusteService ajusteService)
        {
            _turnoService = turnoService;
            _mapper = mapper;
            _dashBoardService = dashBoardService;
            _logger = logger;
            _ticketService = ticketService;
            _ajustesService = ajusteService;
        }

        public IActionResult Turno()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador, Roles.Empleado, Roles.Encargado]);
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnos()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);
            List<VMTurno> VMTurnoList = _mapper.Map<List<VMTurno>>(await _turnoService.List(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = VMTurnoList });
        }


        [HttpGet]
        public async Task<IActionResult> CheckTurnoAbierto()
        {
            var gResponse = new GenericResponse<bool>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var turno = await _turnoService.GetTurnoActual(user.IdTienda);

                gResponse.State = true;
                gResponse.Object = turno != null;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al chequear turno abierto.", _logger);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTurnoActual()
        {
            var gResponse = new GenericResponse<VMTurnoOutput>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var turno = await _turnoService.GetTurnoConVentasPorTipoDeVentaTurno(user.IdTurno);
                var outout = new VMTurnoOutput();

                if (turno != null)
                {
                    VMTurno vmTurnp = null;
                    vmTurnp = _mapper.Map<VMTurno>(turno);

                    if (!vmTurnp.ValidacionRealizada.HasValue || !vmTurnp.ValidacionRealizada.Value)
                    {
                        var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();
                        var dateActual = TimeHelper.GetArgentinaTime();

                        var ventas = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, vmTurnp.IdTurno, user.IdTienda, dateActual);
                        foreach (KeyValuePair<string, decimal> item in ventas.OrderBy(_ => _.Key))
                        {
                            VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                            {
                                Descripcion = item.Key,
                                Total = item.Value
                            });
                        }
                        vmTurnp.VentasPorTipoVentaPreviaValidacion = VentasPorTipoVenta;
                    }

                    var totalMovimiento = turno.MovimientosCaja != null && turno.MovimientosCaja.Any() ? turno.MovimientosCaja.Sum(_ => _.Importe) : 0m;

                    var efectivo = vmTurnp.VentasPorTipoVentaPreviaValidacion.FirstOrDefault(_ => _.Descripcion == "Efectivo");

                    if (efectivo != null && totalMovimiento != 0)
                    {
                        efectivo.Total += totalMovimiento;
                    }
                    else if (efectivo == null && totalMovimiento != 0)
                    {
                        var ventaEfectivo = new VMVentasPorTipoDeVenta()
                        {
                            Descripcion = "Efectivo",
                            Total = totalMovimiento
                        };
                        vmTurnp.VentasPorTipoVentaPreviaValidacion.Add(ventaEfectivo);
                    }

                    if(ajustes.ControlTotalesCierreTurno.HasValue && ajustes.ControlTotalesCierreTurno.Value)
                    {
                        foreach (var item in vmTurnp.VentasPorTipoVentaPreviaValidacion)
                        {
                            item.Total = 0;
                        }
                    }

                    outout.TotalMovimientosCaja = totalMovimiento;
                    outout.Turno = vmTurnp;
                }


                gResponse.Object = outout;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar turno actual.", _logger);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTurno(int idturno)
        {
            var gResponse = new GenericResponse<VMTurnoOutput>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var outout = new VMTurnoOutput();
                var turno = await _turnoService.GetTurno(idturno);
                var vmTurnp = _mapper.Map<VMTurno>(turno);

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();

                vmTurnp.VentasPorTipoVentaPreviaValidacion = VentasPorTipoVenta;

                decimal totalMovimiento = turno.MovimientosCaja != null && turno.MovimientosCaja.Any() ? turno.MovimientosCaja.Sum(_ => _.Importe) : 0m;

                var datos = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, vmTurnp.IdTurno, user.IdTienda, vmTurnp.FechaInicio.Value);

                if (!datos.Any(_ => _.Key == "Efectivo") && (totalMovimiento !=  0 || turno.TotalInicioCaja > 0))
                {
                    datos.Add("Efectivo", 0);
                }

                foreach (KeyValuePair<string, decimal> item in datos)
                {
                    var total = item.Value;
                    if (item.Key == "Efectivo" && (totalMovimiento != 0 || turno.TotalInicioCaja > 0))
                    {
                        total += (int)totalMovimiento;
                        total += (int)turno.TotalInicioCaja;
                    }

                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = total
                    });
                }

                outout.Turno = vmTurnp;
                outout.TotalMovimientosCaja = totalMovimiento;
                outout.ControlCierreCaja = user.ControlCierreCaja;
                gResponse.Object = outout;
                gResponse.State = true;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar turno.", _logger, idturno);
            }

        }

        [HttpPost]
        public async Task<IActionResult> AbrirTurno([FromBody] VMSaveTurno modelTurno)
        {
            var gResponse = new GenericResponse<VMTurno>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);
                var model = new VMTurno()
                {
                    RegistrationUser = user.UserName,
                    RegistrationDate = TimeHelper.GetArgentinaTime(),
                    ObservacionesApertura = modelTurno.ObservacionesApertura,
                    TotalInicioCaja = modelTurno.TotalInicioCaja,
                    FechaInicio = TimeHelper.GetArgentinaTime(),
                    IdTienda = user.IdTienda,
                    TotalCierreCajaReal = 0,
                    TotalCierreCajaSistema = 0,
                    ValidacionRealizada = false
                };


                var nuevoTurno = await _turnoService.Add(_mapper.Map<Turno>(model));

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMTurno>(nuevoTurno);

                await UpdateClaimAsync("Turno", nuevoTurno.IdTurno.ToString());

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cerrar turno.", _logger, modelTurno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CerrarTurno([FromBody] VMTurno modelTurno)
        {
            var gResponse = new GenericResponse<VMImprimir>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                modelTurno.ModificationUser = user.UserName;
                Turno result;

                if (user.ControlCierreCaja)
                {
                    result = await _turnoService.CerrarTurno(_mapper.Map<Turno>(modelTurno));
                }
                else
                {
                    result = await _turnoService.CerrarTurnoSimple(_mapper.Map<Turno>(modelTurno), _mapper.Map<List<VentasPorTipoDeVentaTurno>>(modelTurno.VentasPorTipoVentaPreviaValidacion));
                }

                await UpdateClaimAsync("Turno", null);


                var model = new VMImprimir();

                if (modelTurno.ImpirmirCierreCaja.HasValue && modelTurno.ImpirmirCierreCaja.Value)
                {
                    var ajustes = await _ajustesService.GetAjustes(user.IdTienda);

                    var ticket = await _ticketService.CierreTurno(result, ajustes);

                    model.NombreImpresora = ajustes.NombreImpresora;
                    model.ImagesTicket = new List<Images>();

                    if (!string.IsNullOrEmpty(ajustes.NombreImpresora))
                    {
                        model.Ticket = ticket.Ticket;
                        model.ImagesTicket.AddRange(ticket.ImagesTicket);
                    }
                }

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cerrar turno.", _logger, modelTurno);
            }
        }


        [HttpPost]
        public async Task<IActionResult> ValidarCierreTurno([FromBody] VMTurno modelTurno)
        {
            var gResponse = new GenericResponse<Turno>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                modelTurno.IdTienda = user.IdTienda;

                var turno = await _turnoService.ValidarCierreTurno(_mapper.Map<Turno>(modelTurno), _mapper.Map<List<VentasPorTipoDeVentaTurno>>(modelTurno.VentasPorTipoVentaPreviaValidacion));

                gResponse.State = true;
                gResponse.Object = turno;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al validar cierre de turno.", _logger, modelTurno);
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateTurno([FromBody] VMTurno VMTurno)
        {
            GenericResponse<VMTurno> gResponse = new GenericResponse<VMTurno>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                try
                {
                    VMTurno.ModificationUser = user.UserName;
                    Turno edited_Tienda = await _turnoService.Edit(_mapper.Map<Turno>(VMTurno));

                    VMTurno = _mapper.Map<VMTurno>(edited_Tienda);

                    gResponse.State = true;
                    gResponse.Object = VMTurno;
                }
                catch (Exception ex)
                {
                    gResponse.State = false;
                    gResponse.Message = ex.ToString();
                }

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar turno actual.", _logger, VMTurno);
            }

        }

        [HttpGet]
        public async Task<IActionResult> ImprimirTicketCierre(int idTurno)
        {
            var gResponse = new GenericResponse<VMImprimir>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);
                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var turno = await _turnoService.GetTurno(idTurno);

                var listaVentas = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, turno.IdTurno, user.IdTienda, turno.FechaInicio);

                var model = new VMImprimir
                {
                    NombreImpresora = ajustes.NombreImpresora,
                    ImagesTicket = new List<Images>()
                };

                if (!string.IsNullOrEmpty(ajustes.NombreImpresora))
                {
                    var ticket = await _ticketService.CierreTurno(turno, ajustes);

                    model.Ticket += ticket.Ticket ?? string.Empty;
                    model.ImagesTicket.AddRange(ticket.ImagesTicket);
                }

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al imprimir cierre de turno", _logger, idTurno);
            }
        }
    }
}
