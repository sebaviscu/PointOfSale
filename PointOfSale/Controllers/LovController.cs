using AutoMapper;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class LovController : BaseController
    {
        private readonly ILogger<LovController> _logger;
        private readonly IMapper _mapper;
        private readonly ILovService _lovService;

        public LovController(ILogger<LovController> logger, IMapper mapper, ILovService lovService)
        {
            _logger = logger;
            _mapper = mapper;
            _lovService = lovService;
        }

        /// <summary>
        /// Devuelve los usuarios para un DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDataTable(LovType lovType)
        {
            try
            {
                var list = _mapper.Map<List<VMLov>>(await _lovService.GetLovByType(lovType));
                return StatusCode(StatusCodes.Status200OK, new { data = list });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar los Lov para DataTable");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(LovType lovType)
        {
            var gResponse = new GenericResponse<VMLov>();

            try
            {
                gResponse.ListObject = _mapper.Map<List<VMLov>>(await _lovService.GetLovByType(lovType));
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar todos los lov");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActive(LovType lovType)
        {
            var gResponse = new GenericResponse<VMLov>();

            try
            {
                gResponse.ListObject = _mapper.Map<List<VMLov>>(await _lovService.GetLovActiveByType(lovType));
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al recuperar todos los lov activos");
                return StatusCode(StatusCodes.Status500InternalServerError, new { data = e.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int idLov)
        {
            var gResponse = new GenericResponse<VMLov>();

            try
            {
                var lov = _mapper.Map<VMLov>(await _lovService.GetById(idLov));

                gResponse.Object = lov;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Lov", _logger, idLov);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VMLov model)
        {

            var gResponse = new GenericResponse<VMLov>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                model.RegistrationDate = TimeHelper.GetArgentinaTime();

                var lov_creado = await _lovService.Add(_mapper.Map<Lov>(model));

                gResponse.Object = _mapper.Map<VMLov>(lov_creado);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear lov", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] VMLov model)
        {

            var gResponse = new GenericResponse<VMLov>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
                model.ModificationDate = TimeHelper.GetArgentinaTime();
                model.ModificationUser = user.UserName;

                var lov_edited = await _lovService.Edit(_mapper.Map<Lov>(model));

                gResponse.Object = _mapper.Map<VMLov>(lov_edited); ;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar lov", _logger, model);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int idLov)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _lovService.Delete(idLov);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al borrar lov", _logger, idLov);
            }

        }
    }
}
