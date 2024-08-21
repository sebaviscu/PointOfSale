using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.IO;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class GastosController : BaseController
    {
        private readonly IGastosService _GastosService;
        private readonly IMapper _mapper;
        private readonly ILogger<GastosController> _logger;

        public GastosController(IGastosService GastosService, IMapper mapper, ILogger<GastosController> logger)
        {
            _GastosService = GastosService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Gastos()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recupera gastos para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetGastos(bool visionGlobal)
        {
            var gResponse = new GenericResponse<List<VMGastos>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var idtienda = visionGlobal ? 0 : user.IdTienda;

                List<VMGastos> vmGastosList = _mapper.Map<List<VMGastos>>(await _GastosService.List(idtienda));
                return StatusCode(StatusCodes.Status200OK, new { data = vmGastosList });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de gastos", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGastos([FromBody] VMGastos model)
        {

            GenericResponse<VMGastos> gResponse = new GenericResponse<VMGastos>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                model.RegistrationDate = TimeHelper.GetArgentinaTime();
                model.RegistrationUser = user.UserName;
                var gasto = _mapper.Map<Gastos>(model);
                gasto.IdTienda = user.IdTienda;
                Gastos Gastos_created = await _GastosService.Add(gasto);

                model = _mapper.Map<VMGastos>(Gastos_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear gastos.", _logger, model.ToJson());
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateGastos([FromBody] VMGastos model)
        {

            GenericResponse<VMGastos> gResponse = new GenericResponse<VMGastos>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                model.ModificationUser = user.UserName;

                Gastos edited_Gastos = await _GastosService.Edit(_mapper.Map<Gastos>(model));
                edited_Gastos.RegistrationUser = user.UserName;

                model = _mapper.Map<VMGastos>(edited_Gastos);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar gastos.", _logger, model.ToJson());
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteGastos(int idGastos)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                gResponse.State = await _GastosService.Delete(idGastos);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar gastos.", _logger, idGastos.ToJson());
            }
        }

        /// <summary>
        /// Recupera tipo de gastos para Selects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTipoDeGasto()
        {
            var gResponse = new GenericResponse<List<VMTipoDeGasto>>();

            try
            {
                gResponse.State = true;
                gResponse.Object = _mapper.Map<List<VMTipoDeGasto>>(await _GastosService.ListTipoDeGasto());
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar tipo de gastos.", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTipoDeGastos([FromBody] VMTipoDeGasto model)
        {

            GenericResponse<VMTipoDeGasto> gResponse = new GenericResponse<VMTipoDeGasto>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                TipoDeGasto Gastos_created = await _GastosService.AddTipoDeGasto(_mapper.Map<TipoDeGasto>(model));

                model = _mapper.Map<VMTipoDeGasto>(Gastos_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear tipo de gastos.", _logger, model.ToJson());
            }
        }

        //[HttpPut]
        //public async Task<IActionResult> UpdateGastos([FromBody] VMGastos model)
        //{
        //    var user = ValidarAutorizacion([Roles.Administrador]);

        //    GenericResponse<VMGastos> gResponse = new GenericResponse<VMGastos>();
        //    try
        //    {
        //        var ss = _mapper.Map<Gastos>(model);
        //        Gastos edited_Gastos = await _GastosService.Edit(_mapper.Map<Gastos>(model));
        //        edited_Gastos.ModificationUser = user.UserName;

        //        model = _mapper.Map<VMGastos>(edited_Gastos);

        //        gResponse.State = true;
        //        gResponse.Object = model;
        //    }
        //    catch (Exception ex)
        //    {
        //        gResponse.State = false;
        //        gResponse.Message = ex.ToString();
        //    }

        //    return StatusCode(StatusCodes.Status200OK, gResponse);
        //}


        [HttpDelete]
        public async Task<IActionResult> DeleteTipoDeGastos(int idTipoGastos)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                gResponse.State = await _GastosService.DeleteTipoDeGasto(idTipoGastos);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar tipo de gastos.", _logger, idTipoGastos.ToJson());
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetGastosTablaDinamica(bool visionGlobal)
        {
            var gResponse = new GenericResponse<List<VMGastosTablaDinamica>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var idtienda = visionGlobal ? 0 : user.IdTienda;

                var listUsers = _mapper.Map<List<VMGastosTablaDinamica>>(await _GastosService.ListGastosForTablaDinamica(user.IdTienda));

                gResponse.Object = listUsers.OrderByDescending(_ => _.RegistrationUser).ThenByDescending(_ => _.Gasto).ThenByDescending(_ => _.Tipo_Gasto).ToList();
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar gastos para tabla dinamica.", _logger);
            }
        }
    }
}
