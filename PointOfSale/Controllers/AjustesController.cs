using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class AjustesController : BaseController
    {
        private readonly ILogger<AjustesController> _logger;
        private readonly IMapper _mapper;
        private readonly IAjusteService _ajusteService;
        private readonly IAfipService _afipService;
        private readonly ITurnoService _turnoService;
        private readonly INotificationService _notificationService;
        private readonly ITicketService _ticketService;

        public AjustesController(ILogger<AjustesController> logger, IMapper mapper, IAjusteService ajusteService, IAfipService afipService, ITurnoService turnoService, INotificationService notificationService, ITicketService ticketService)
        {
            _ajusteService = ajusteService;
            _logger = logger;
            _mapper = mapper;
            _afipService = afipService;
            _turnoService = turnoService;
            _notificationService = notificationService;
            _ticketService = ticketService;
        }

        public IActionResult Index()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetAjusteWeb()
        {
            var gResponse = new GenericResponse<VMAjustesWeb>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmAjusteWeb = _mapper.Map<VMAjustesWeb>(await _ajusteService.GetAjustesWeb());

                gResponse.Object = vmAjusteWeb;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajuste web", _logger);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAjuste()
        {
            var gResponse = new GenericResponse<VMAjustes>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmAjuste = _mapper.Map<VMAjustes>(await _ajusteService.GetAjustes(user.IdTienda));

                gResponse.Object = vmAjuste;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajuste", _logger);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAjuste([FromForm] IFormFile Certificado, [FromForm] string modelFacturacion, [FromForm] string modelAjustes, [FromForm] string ModelWeb)
        {
            var gResponse = new GenericResponse<string>();
            try
            {
                var model = JsonConvert.DeserializeObject<VMAjustes>(modelAjustes);
                var modelWeb = JsonConvert.DeserializeObject<VMAjustesWeb>(ModelWeb);
                var vmModelFacturacion = JsonConvert.DeserializeObject<VMAjustesFacturacion>(modelFacturacion);

                var user = ValidarAutorizacion([Roles.Administrador]);

                model.ModificationUser = user.UserName;
                model.IdTienda = user.IdTienda;
                vmModelFacturacion.ModificationUser = user.UserName;
                vmModelFacturacion.IdTienda = user.IdTienda;

                modelWeb.ModificationUser = user.UserName;
                modelWeb.IdTienda = user.IdTienda;

                _ = await _ajusteService.EditWeb(_mapper.Map<AjustesWeb>(modelWeb));

                _ = await _ajusteService.Edit(_mapper.Map<Ajustes>(model));
                var pathFile = string.Empty;
                if (Certificado != null)
                {
                    var oldAjustes = await _ajusteService.GetAjustesFacturacion(user.IdTienda);

                    pathFile = await _afipService.ReplaceCertificateAsync(Certificado, user.IdTienda, oldAjustes.CertificadoNombre);

                    vmModelFacturacion.CertificadoNombre = Certificado.FileName;
                    if (!string.IsNullOrEmpty(vmModelFacturacion.CertificadoPassword))
                    {
                        var cert = _afipService.GetCertificateAfipInformation(pathFile, vmModelFacturacion.CertificadoPassword);
                        if (cert != null)
                        {
                            vmModelFacturacion.CertificadoFechaCaducidad = cert.FechaCaducidad;
                            vmModelFacturacion.CertificadoFechaInicio = cert.FechaInicio;
                            vmModelFacturacion.Cuit = Convert.ToInt64(cert.Cuil);
                        }
                    }
                }

                _ = await _ajusteService.EditFacturacion(_mapper.Map<AjustesFacturacion>(vmModelFacturacion));

                gResponse.State = true;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajustes", _logger, model: null, ("ModelFacturacion", modelFacturacion), ("ModelAjustes", modelAjustes));
            }
        }

        /// <summary>
        /// Recupera ajustes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAjustesVentas()
        {
            var gResponse = new GenericResponse<VMAjustesSale?>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);

                var ajuste = await _ajusteService.GetAjustes(user.IdTienda);
                var vmAjuste = _mapper.Map<VMAjustesSale>(ajuste);

                vmAjuste.NeedControl = user.IdRol == (int)Roles.Administrador
                    ? false
                    : ajuste.ControlEmpleado ?? false;

                vmAjuste.ListaPrecios = user.ListaPrecios;
                var turno = await _turnoService.GetTurnoActual(user.IdTienda);

                vmAjuste.ExisteTurno = turno != null;
                vmAjuste.User = user.UserName;

                gResponse.State = true;
                gResponse.Object = vmAjuste;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajustes", _logger);
            }
        }


        /// <summary>
        /// Recupera ajustes para saber aumento web
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAjustesWeb()
        {
            var gResponse = new GenericResponse<VMAjustesWebReducido>();
            try
            {
                var ajuste = await _ajusteService.GetAjustesWeb();

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMAjustesWebReducido>(ajuste);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar aumento web", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateSecurityCode(string encryptedCode, string detalle)
        {
            var gResponse = new GenericResponse<bool>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);

                var ajuste = await _ajusteService.GetAjustes(user.IdTienda);

                var codigo = ajuste.CodigoSeguridad != null ? ajuste.CodigoSeguridad : string.Empty;

                var notific = new Notifications(user.UserName, detalle);
                await _notificationService.Save(notific);

                gResponse.State = true;
                gResponse.Object = encryptedCode == codigo;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al validar codigo de seguridad", _logger, encryptedCode);
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
                return HandleException(ex, "Error al recuperar ajuste de facturacion", _logger);
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> GetValidateCertificate()
        //{
        //    var gResponse = new GenericResponse<string>();
        //    try
        //    {
        //        var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);
        //        var ajustes = await _ajusteService.GetAjustesFacturacion(user.IdTienda);
        //        var resp = _afipService.ValidateCertificate(ajustes);

        //        gResponse.Object = resp;
        //        gResponse.State = resp == string.Empty;
        //        return StatusCode(StatusCodes.Status200OK, gResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        return HandleException(ex, "Error en el certificado", _logger);
        //    }
        //}

        //[HttpPut]
        //public async Task<IActionResult> UpdateAjustesFacturacion([FromForm] IFormFile Certificado, [FromForm] string model)
        //{

        //    GenericResponse<VMAjustesFacturacion> gResponse = new GenericResponse<VMAjustesFacturacion>();
        //    try
        //    {
        //        var vmModel = JsonConvert.DeserializeObject<VMAjustesFacturacion>(model);
        //        var user = ValidarAutorizacion([Roles.Administrador]);

        //        if (Certificado != null)
        //        {
        //            var oldAjustes = await _ajusteService.GetAjustesFacturacion(user.IdTienda);

        //            var pathFile = await _afipService.ReplaceCertificateAsync(Certificado, user.IdTienda, oldAjustes.CertificadoNombre);

        //            vmModel.CertificadoNombre = Certificado.FileName;
        //            if (!string.IsNullOrEmpty(vmModel.CertificadoPassword))
        //            {
        //                var cert = _afipService.GetCertificateAfipInformation(pathFile, vmModel.CertificadoPassword);
        //                if (cert != null)
        //                {
        //                    vmModel.CertificadoFechaCaducidad = cert.FechaCaducidad;
        //                    vmModel.CertificadoFechaInicio = cert.FechaInicio;
        //                    vmModel.Cuit = Convert.ToInt64(cert.Cuil);
        //                }
        //            }
        //        }

        //        vmModel.ModificationUser = user.UserName;
        //        vmModel.IdTienda = user.IdTienda;
        //        var ajustes = await _ajusteService.EditFacturacion(_mapper.Map<AjustesFacturacion>(vmModel));

        //        gResponse.State = true;
        //        gResponse.Object = _mapper.Map<VMAjustesFacturacion>(ajustes);
        //        gResponse.Message = string.Empty;

        //    }
        //    catch (Exception ex)
        //    {
        //        return HandleException(ex, "Error al actualizar ajustes de facturación", _logger, model);
        //    }

        //    return StatusCode(StatusCodes.Status200OK, gResponse);
        //}

        [HttpPut]
        public async Task<IActionResult> UpdateCertificateInformation([FromForm] IFormFile Certificado, [FromForm] string password)
        {

            GenericResponse<VMAjustesFacturacion> gResponse = new GenericResponse<VMAjustesFacturacion>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var vmModel = new VMAjustesFacturacion();

                if (Certificado != null)
                {

                    var filePath = Path.Combine(Path.GetTempPath(), Certificado.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Certificado.CopyToAsync(stream);
                    }

                    vmModel.CertificadoNombre = Certificado.FileName;
                    if (!string.IsNullOrEmpty(password))
                    {
                        var cert = _afipService.GetCertificateAfipInformation(filePath, password);
                        if (cert != null)
                        {
                            vmModel.CertificadoFechaCaducidad = cert.FechaCaducidad;
                            vmModel.CertificadoFechaInicio = cert.FechaInicio;
                            vmModel.Cuit = Convert.ToInt64(cert.Cuil);
                        }
                    }

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                //vmModel.ModificationUser = user.UserName;
                //vmModel.IdTienda = user.IdTienda;
                //var ajustes = await _ajusteService.UpdateCertificateInfo(_mapper.Map<AjustesFacturacion>(vmModel));

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMAjustesFacturacion>(vmModel);
                gResponse.Message = string.Empty;

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar informacion del certificado.", _logger);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> PrintTicketTests()
        {
            var gResponse = new GenericResponse<VMSale>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var ajustes = await _ajusteService.GetAjustes(user.IdTienda);

                var model = new VMSale
                {
                    NombreImpresora = ajustes.NombreImpresora,
                    ImagesTicket = new List<Images>()
                };


                var listDetailsSale = new List<DetailSale>()
                {
                    new DetailSale()
                    {
                        DescriptionProduct = "Articulo 1",
                        Price = 25,
                        Quantity = 2,
                        Total = 50,
                        Iva= 21m
                    },
                    new DetailSale()
                    {
                        DescriptionProduct = "Articulo 2",
                        Price = 10,
                        Quantity = 1.3m,
                        Total = 13,
                        Iva= 10.5m
                    }
                };


                if (!string.IsNullOrEmpty(ajustes.NombreImpresora))
                {
                    var ticket = await _ticketService.TicketTest(listDetailsSale, ajustes);

                    model.Ticket += ticket.Ticket;
                    model.ImagesTicket.AddRange(ticket.ImagesTicket);
                }

                gResponse.Object = model;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar ajuste de facturacion", _logger);
            }
        }
    }
}
