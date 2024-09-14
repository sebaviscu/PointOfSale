using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
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
                var list = _mapper.Map<List<VMFormatosVenta>>(await _tablaService.ListFormatosVenta());
                return StatusCode(StatusCodes.Status200OK, new { data = list });
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
                var list = _mapper.Map<List<VMFormatosVenta>>(await _tablaService.ListFormatosVentaActive());
                return StatusCode(StatusCodes.Status200OK, new { data = list });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Formatos venta", _logger);
            }

        }
    }
}
