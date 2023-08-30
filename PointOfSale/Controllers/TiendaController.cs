using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
			List<VMTienda> vmTiendaList = _mapper.Map<List<VMTienda>>(await _TiendaService.List());
			return StatusCode(StatusCodes.Status200OK, new { data = vmTiendaList });
		}

		[HttpPost]
		public async Task<IActionResult> CreateTienda([FromForm] IFormFile photo, [FromForm] string model)
		{
			ValidarAutorizacion(new Roles[] { Roles.Administrador });

			GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
			try
			{
                VMTienda vmTienda = JsonConvert.DeserializeObject<VMTienda>(model);

				if (photo != null)
				{
					using (var ms = new MemoryStream())
					{
						photo.CopyTo(ms);
						var fileBytes = ms.ToArray();
						vmTienda.Logo = fileBytes;
					}
				}
				else
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
			var user = ValidarAutorizacion(new Roles[] { Roles.Administrador});

			GenericResponse<VMTienda> gResponse = new GenericResponse<VMTienda>();
			try
			{
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

				vmTienda.ModificationUser = user.UserName;
				Tienda edited_Tienda = await _TiendaService.Edit(_mapper.Map<Tienda>(vmTienda));

                vmTienda = _mapper.Map<VMTienda>(edited_Tienda);

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


		[HttpDelete]
		public async Task<IActionResult> DeleteTienda(int idTienda)
		{
			ValidarAutorizacion(new Roles[] { Roles.Administrador});

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
