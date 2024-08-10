using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;
using NuGet.Protocol;
using Newtonsoft.Json;
using PointOfSale.Business.Services;

namespace PointOfSale.Controllers
{
    public class TiendaController : BaseController
    {
        private readonly ITiendaService _TiendaService;
        private readonly IMapper _mapper;
        private readonly ILogger<TiendaController> _logger;
        private readonly IAjusteService _ajusteService;

        public TiendaController(ITiendaService TiendaService, IMapper mapper, ILogger<TiendaController> logger, IAjusteService ajusteService)
        {
            _TiendaService = TiendaService;
            _mapper = mapper;
            _logger = logger;
            _ajusteService = ajusteService;
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
                return HandleException(ex, "Error al recuperar Tiendas", _logger);
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateTienda([FromForm] IFormFile Logo, [FromForm] string model)
        {

            GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
            var vmTienda = JsonConvert.DeserializeObject<VMTienda>(model);

            try
            {
                ValidarAutorizacion([Roles.Administrador]);

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
                await _ajusteService.CreateAjsutes(Tienda_created.IdTienda);

                vmTienda = _mapper.Map<VMTienda>(Tienda_created);

                gResponse.State = true;
                gResponse.Object = vmTienda;
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear Tiendas", _logger, vmTienda.ToJson());
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTienda([FromForm] IFormFile Logo, [FromForm] string model)
        {

            GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
            var vmModel = JsonConvert.DeserializeObject<VMTienda>(model);
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

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
                return HandleException(ex, "Error al actualizar Tiendas", _logger, model.ToJson());
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
                return HandleException(ex, "Error al eliminar Tienda", _logger, idTienda.ToJson());
            }

        }
    }
}
