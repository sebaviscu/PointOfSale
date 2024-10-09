using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Auditoria;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class LicenciaController : BaseController
    {
        private readonly ILogger<LicenciaController> _logger;
        private readonly IMapper _mapper;
        private readonly IPagoEmpresaService _pagoEmpresaService;
        private readonly IUserService _userService;

        public LicenciaController(ILogger<LicenciaController> logger, IMapper mapper, IPagoEmpresaService pagoEmpresaService, IUserService userService)
        {
            _logger = logger;
            _mapper = mapper;
            _pagoEmpresaService = pagoEmpresaService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);
            ViewData["superUser"] = await _userService.CheckSuperUser(user.IdUsuario);

            return View();
        }

        /// <summary>
        /// Devuelve los usuarios para un DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDataTable()
        {
            try
            {
                var queryList = _pagoEmpresaService.List();
                var pagosEmpresa = _mapper.Map<List<VMPagoEmpresa>>(await queryList.ToListAsync());

                return StatusCode(StatusCodes.Status200OK, new { data = pagosEmpresa });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar los pagos licencia para DataTable");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLicencia()
        {

            var gResponse = new GenericResponse<VMEmpresa>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var empresa = _mapper.Map<VMEmpresa>(await _pagoEmpresaService.GetEmpresa());

                gResponse.State = true;
                gResponse.Object = empresa;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Licencia.", _logger);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckLicencia()
        {
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var result = await _userService.CheckSuperUser(user.IdUsuario);

                if (result)
                {
                    return View("UpdateLicencia");
                }
                else
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Licencia.", _logger);
            }
        }

        public async Task<IActionResult> UpdateLicencia()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);

            var result = await _userService.CheckSuperUser(user.IdUsuario);

            return result ? View() : StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePagoLicencia([FromBody] VMPagoEmpresa model)
        {

            GenericResponse<VMPagoEmpresa> gResponse = new GenericResponse<VMPagoEmpresa>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                model.RegistrationDate = TimeHelper.GetArgentinaTime();
                model.RegistrationUser = user.UserName;

                var empresa = await _pagoEmpresaService.GetEmpresa();

                model.IdEmpresa = empresa.Id;

                var gasto = _mapper.Map<PagoEmpresa>(model);
                var pago = await _pagoEmpresaService.Add(gasto);

                model = _mapper.Map<VMPagoEmpresa>(pago);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear pago de licencia.", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdatePagoLicencia([FromBody] VMPagoEmpresa model)
        {

            GenericResponse<VMPagoEmpresa> gResponse = new GenericResponse<VMPagoEmpresa>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                model.ModificationUser = user.UserName;
                model.ModificationDate = TimeHelper.GetArgentinaTime();

                var pago = await _pagoEmpresaService.Edit(_mapper.Map<PagoEmpresa>(model));

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMPagoEmpresa>(pago);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar pago de licencia.", _logger, model);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeletePagoLicencia(int idPago)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                gResponse.State = await _pagoEmpresaService.Delete(idPago);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar pago de licencia.", _logger, idPago);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmpresa([FromBody] VMEmpresa model)
        {

            GenericResponse<VMEmpresa> gResponse = new GenericResponse<VMEmpresa>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                model.ModificationUser = user.UserName;
                model.ModificationDate = TimeHelper.GetArgentinaTime();

                gResponse.State = await _pagoEmpresaService.UpdateEmpresa(_mapper.Map<Empresa>(model));
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar Empresa.", _logger, model);
            }
        }
    }
}
