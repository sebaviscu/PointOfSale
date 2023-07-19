using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Business.Utilities.Enum;

namespace PointOfSale.Controllers
{
	[Authorize]
	public class TiendaController : BaseController
	{
		private readonly ITiendaService _TiendaService;
		private readonly IMapper _mapper;
		public TiendaController(ITiendaService TiendaService, IMapper mapper)
		{
			_TiendaService = TiendaService;
			_mapper = mapper;
		}

		public IActionResult Tienda()
		{
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> GetTienda()
		{
			var dd = await _TiendaService.List();
			List<VMTienda> vmTiendaList = _mapper.Map<List<VMTienda>>(await _TiendaService.List());
			return StatusCode(StatusCodes.Status200OK, new { data = vmTiendaList });
		}

		[HttpPost]
		public async Task<IActionResult> CreateTienda([FromBody] VMTienda model)
		{
			GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
			try
			{
				Tienda Tienda_created = await _TiendaService.Add(_mapper.Map<Tienda>(model));

				model = _mapper.Map<VMTienda>(Tienda_created);

				gResponse.State = true;
				gResponse.Object = model;
			}
			catch (Exception ex)
			{
				gResponse.State = false;
				gResponse.Message = ex.Message;
			}

			return StatusCode(StatusCodes.Status200OK, gResponse);
		}

		[HttpPut]
		public async Task<IActionResult> UpdateTienda([FromBody] VMTienda model)
		{
			var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

			GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
			try
			{
				model.ModificationUser = user.UsuarioId;

				Tienda edited_Tienda = await _TiendaService.Edit(_mapper.Map<Tienda>(model));

				model = _mapper.Map<VMTienda>(edited_Tienda);

				gResponse.State = true;
				gResponse.Object = model;
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
			var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

			GenericResponse<string> gResponse = new GenericResponse<string>();
			try
			{
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
