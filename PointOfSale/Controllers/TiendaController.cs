using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class TiendaController : BaseController
    {
        private readonly ITiendaService _TiendaService;
        private readonly IMapper _mapper;
        private readonly ILogger<TiendaController> _logger;

        public TiendaController(ITiendaService TiendaService, IMapper mapper, ILogger<TiendaController> logger)
        {
            _TiendaService = TiendaService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Tienda()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetTienda()
        {
            try
            {
                List<VMTienda> vmTiendaList = _mapper.Map<List<VMTienda>>(await _TiendaService.List());
                return StatusCode(StatusCodes.Status200OK, new { data = vmTiendaList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOneTienda()
        {
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var tienda = _mapper.Map<VMTienda>(await _TiendaService.Get(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = tienda });
            }
            catch (Exception e)
            {

                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateTienda(/*[FromForm] IFormFile photo,*/ [FromBody] VMTienda vmTienda)
        {

            GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
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

                vmTienda = _mapper.Map<VMTienda>(Tienda_created);

                gResponse.State = true;
                gResponse.Object = vmTienda;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTienda([FromBody] VMTienda vmTienda)
        {

            GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                //VMTienda vmTienda = JsonConvert.DeserializeObject<VMTienda>(model);

                //if (photo != null)
                //{
                //	using (var ms = new MemoryStream())
                //	{
                //		photo.CopyTo(ms);
                //		var fileBytes = ms.ToArray();
                //		vmTienda.Logo = fileBytes;
                //	}
                //}
                //else
                //	vmTienda.Logo = null;
                var tiendaOld = _mapper.Map<VMTienda>(await _TiendaService.Get(vmTienda.IdTienda));

                vmTienda.ModificationUser = user.UserName;
                Tienda edited_Tienda = await _TiendaService.Edit(_mapper.Map<Tienda>(vmTienda));

                if (tiendaOld.IdListaPrecio != edited_Tienda.IdListaPrecio)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    gResponse.State = true;
                    gResponse.Message = "Como se ha cambiado la lista de precios, se debe iniciar sesion nuevamente.";
                }

                vmTienda = _mapper.Map<VMTienda>(edited_Tienda);

                gResponse.State = true;
                gResponse.Object = vmTienda;
                gResponse.Message = string.Empty;

            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
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
