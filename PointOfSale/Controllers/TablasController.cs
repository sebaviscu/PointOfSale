using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class TablasController : BaseController
    {

        private readonly ITablaService _tablaService;
        private readonly IMapper _mapper;
        private readonly ILogger<TablasController> _logger;

        public TablasController(ITablaService tablaService, IMapper mapper, ILogger<TablasController> logger)
        {
            _tablaService = tablaService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recupero Formatos venta para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFormtadoVenta()
        {
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                var list = _mapper.Map<List<VMFormatosVenta>>(await _tablaService.List());
                return StatusCode(StatusCodes.Status200OK, new { data = list.OrderBy(_=>_.Valor) });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Formatos venta", _logger);
            }

        }

        /// <summary>
        /// Recupero Formatos venta para select
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFormtadoVentaActive()
        {
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                var list = _mapper.Map<List<VMFormatosVenta>>(await _tablaService.ListActive());
                return StatusCode(StatusCodes.Status200OK, new { data = list.OrderBy(_ => _.Valor) });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Formatos venta", _logger);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateFormatosVenta([FromBody] VMFormatosVenta model)
        {

            var gResponse = new GenericResponse<VMFormatosVenta>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var category_created = await _tablaService.Add(_mapper.Map<FormatosVenta>(model));

                model = _mapper.Map<VMFormatosVenta>(category_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear Formatos Venta.", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateFormatosVenta([FromBody] VMFormatosVenta model)
        {

            var gResponse = new GenericResponse<VMFormatosVenta>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                FormatosVenta category_created = await _tablaService.Edit(_mapper.Map<FormatosVenta>(model));

                model = _mapper.Map<VMFormatosVenta>(category_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear FormatosVenta.", _logger, model);
            }

        }


        [HttpDelete]
        public async Task<IActionResult> DeleteFormatosVenta(int idFormatosVenta)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                gResponse.State = await _tablaService.Delete(idFormatosVenta);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al borrar categoria.", _logger, idFormatosVenta);
            }
        }
    }
}
