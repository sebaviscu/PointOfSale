using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;
using NuGet.Protocol;
using System.Security.Cryptography.X509Certificates;
using PointOfSale.Business.Services;
using Newtonsoft.Json;
using PintOfSale.FileStorageService.Servicios;

namespace PointOfSale.Controllers
{
    public class TiendaController : BaseController
    {
        private readonly ITiendaService _TiendaService;
        private readonly IMapper _mapper;
        private readonly ILogger<TiendaController> _logger;
        private readonly IFileStorageService _fileStorageService;

        public TiendaController(ITiendaService TiendaService, IMapper mapper, ILogger<TiendaController> logger, IFileStorageService fileStorageService)
        {
            _TiendaService = TiendaService;
            _mapper = mapper;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Tienda()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetTienda()
        {
            var gResponse = new GenericResponse<VMTienda>();

            try
            {
                var claimuser = HttpContext.User;

                var idTienda = Convert.ToInt32(claimuser.Claims
                    .Where(c => c.Type == "Tienda")
                    .Select(c => c.Value).SingleOrDefault());

                var vmTiendaList = _mapper.Map<List<VMTienda>>(await _TiendaService.List());


                if (idTienda != null && idTienda != 0)
                {
                    var tiendaActual = vmTiendaList.FirstOrDefault(_ => _.IdTienda == idTienda);
                    if (tiendaActual != null)
                    {
                        tiendaActual.TiendaActual = true;
                    }
                }

                return StatusCode(StatusCodes.Status200OK, new { data = vmTiendaList });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar Tiendas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetTiendoWithCertificado(int idTienda)
        {
            var gResponse = new GenericResponse<VMTienda>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var tienda = _mapper.Map<VMTienda>(await _TiendaService.GetWithPassword(user.IdTienda));

                if (idTienda == user.IdTienda && !string.IsNullOrEmpty(tienda.CertificadoPassword))
                {
                    var certificatePath = await _fileStorageService.ObtenerRutaCertificadoAsync(idTienda);
                    tienda.vMX509Certificate2 = _mapper.Map<VMX509Certificate2>(_TiendaService.GetCertificateAfipInformation(certificatePath, tienda.CertificadoPassword));
                }

                gResponse.Object = tienda;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar una tienda";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOneTienda()
        {
            var gResponse = new GenericResponse<VMTienda>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var tienda = _mapper.Map<VMTienda>(await _TiendaService.Get(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = tienda });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar una tienda";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateTienda([FromForm] IFormFile Certificado, [FromForm] IFormFile Logo, [FromForm] string model)
        {

            GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
            var vmTienda = JsonConvert.DeserializeObject<VMTienda>(model);

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                //if (Certificado != null)
                //{
                //    var certificadoPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificados", Certificado.FileName);
                //    using (var stream = new FileStream(certificadoPath, FileMode.Create))
                //    {
                //        await Certificado.CopyToAsync(stream);
                //    }
                //    // Guarda la ruta del certificado en el objeto Tienda si es necesario
                //    //vmTienda.CertificadoPath = certificadoPath;
                //}

                //VMTienda vmTienda = JsonConvert.DeserializeObject<VMTienda>(model);

                //if (photo != null)
                //{
                //    using (var ms = new MemoryStream())
                //    {
                //        photo.CopyTo(ms);
                //        var fileBytes = ms.ToArray();
                //        vmTienda.Logo = fileBytes;
                //    }
                //}
                //else
                vmTienda.Logo = null;

                Tienda Tienda_created = await _TiendaService.Add(_mapper.Map<Tienda>(vmTienda));

                vmTienda = _mapper.Map<VMTienda>(Tienda_created);

                gResponse.State = true;
                gResponse.Object = vmTienda;
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al crear Tiendas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, vmTienda.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTienda([FromForm] IFormFile Certificado, [FromForm] IFormFile Logo, [FromForm] string model)
        {

            GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
            var vmModel = JsonConvert.DeserializeObject<VMTienda>(model);
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                if (Certificado != null)
                {
                    var nuevoProyectoPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "PintOfSale.FileStorageService");
                    var certificadoDirectory = Path.Combine(nuevoProyectoPath, "Certificados", user.IdTienda.ToString() + "_Tienda");

                    await _fileStorageService.ReplaceFileAsync(Certificado, certificadoDirectory);

                    _ = await _TiendaService.EditCertificate(vmModel.IdTienda, Certificado.FileName);
                }

                //if (Logo != null)
                //{
                //    using (var ms = new MemoryStream())
                //    {
                //        Logo.CopyTo(ms);
                //        var fileBytes = ms.ToArray();
                //        vmModel.Logo = fileBytes;
                //    }
                //}
                //else
                //    vmModel.Logo = null;

                vmModel.ModificationUser = user.UserName;
                Tienda edited_Tienda = await _TiendaService.Edit(_mapper.Map<Tienda>(vmModel));

                await UpdateClaimAsync("ListaPrecios", ((int)edited_Tienda.IdListaPrecio).ToString());

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMTienda>(edited_Tienda);
                gResponse.Message = string.Empty;

            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar Tiendas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteTienda(int idTienda)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                gResponse.State = await _TiendaService.Delete(idTienda);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al eliminar Tiendas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, idTienda.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }
    }
}
