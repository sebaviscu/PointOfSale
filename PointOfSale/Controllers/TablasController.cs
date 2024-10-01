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
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;

        public TablasController(ITablaService tablaService, IMapper mapper, ILogger<TablasController> logger, ITypeDocumentSaleService typeDocumentSaleService)
        {
            _tablaService = tablaService;
            _mapper = mapper;
            _logger = logger;
            _typeDocumentSaleService = typeDocumentSaleService;
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

        /// <summary>
        /// Recupero las formas de pago para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTipoVenta()
        {

            List<VMTypeDocumentSale> listUsers = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = listUsers });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTipoVenta([FromBody] VMTypeDocumentSale model)
        {
            var gResponse = new GenericResponse<VMTypeDocumentSale>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var usuario_creado = await _typeDocumentSaleService.Add(_mapper.Map<TypeDocumentSale>(model));

                gResponse.Object = _mapper.Map<VMTypeDocumentSale>(usuario_creado);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear forma de pago", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateTipoVenta([FromBody] VMTypeDocumentSale model)
        {

            GenericResponse<VMTypeDocumentSale> gResponse = new GenericResponse<VMTypeDocumentSale>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                TypeDocumentSale user_edited = await _typeDocumentSaleService.Edit(_mapper.Map<TypeDocumentSale>(model));

                gResponse.Object = _mapper.Map<VMTypeDocumentSale>(user_edited);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar forma de pago", _logger, model);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTipoVenta(int idTypeDocumentSale)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                gResponse.State = await _typeDocumentSaleService.Delete(idTypeDocumentSale);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar forma de pago", _logger, idTypeDocumentSale);
            }
        }
    }
}
