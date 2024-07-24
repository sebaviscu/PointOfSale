using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class GastosController : BaseController
    {
        private readonly IGastosService _GastosService;
        private readonly IMapper _mapper;
        public GastosController(IGastosService GastosService, IMapper mapper)
        {
            _GastosService = GastosService;
            _mapper = mapper;
        }

        public IActionResult Gastos()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetGastos()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            List<VMGastos> vmGastosList = _mapper.Map<List<VMGastos>>(await _GastosService.List(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = vmGastosList });

        }

        [HttpPost]
        public async Task<IActionResult> CreateGastos([FromBody] VMGastos model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMGastos> gResponse = new GenericResponse<VMGastos>();
            try
            {
                model.RegistrationDate = TimeHelper.GetArgentinaTime();
                model.RegistrationUser = user.UserName;
                var gasto = _mapper.Map<Gastos>(model);
                gasto.IdTienda = user.IdTienda;
                Gastos Gastos_created = await _GastosService.Add(gasto);

                model = _mapper.Map<VMGastos>(Gastos_created);

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
        public async Task<IActionResult> UpdateGastos([FromBody] VMGastos model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMGastos> gResponse = new GenericResponse<VMGastos>();
            try
            {
                model.ModificationUser = user.UserName;

                Gastos edited_Gastos = await _GastosService.Edit(_mapper.Map<Gastos>(model));
                edited_Gastos.RegistrationUser = user.UserName;

                model = _mapper.Map<VMGastos>(edited_Gastos);

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
        public async Task<IActionResult> DeleteGastos(int idGastos)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _GastosService.Delete(idGastos);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpGet]
        public async Task<IActionResult> GetTipoDeGasto()
        {
            List<VMTipoDeGasto> vmGastosList = _mapper.Map<List<VMTipoDeGasto>>(await _GastosService.ListTipoDeGasto());
            return StatusCode(StatusCodes.Status200OK, new { data = vmGastosList });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTipoDeGastos([FromBody] VMTipoDeGasto model)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMTipoDeGasto> gResponse = new GenericResponse<VMTipoDeGasto>();
            try
            {
                TipoDeGasto Gastos_created = await _GastosService.AddTipoDeGasto(_mapper.Map<TipoDeGasto>(model));

                model = _mapper.Map<VMTipoDeGasto>(Gastos_created);

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

        //[HttpPut]
        //public async Task<IActionResult> UpdateGastos([FromBody] VMGastos model)
        //{
        //    var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

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
        //        gResponse.Message = ex.Message;
        //    }

        //    return StatusCode(StatusCodes.Status200OK, gResponse);
        //}


        [HttpDelete]
        public async Task<IActionResult> DeleteTipoDeGastos(int IdTipoGastoss)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _GastosService.DeleteTipoDeGasto(IdTipoGastoss);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpGet]
        public async Task<IActionResult> GetGastosTablaDinamica()
        {
            try
            {

                var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });
                var listUsers = _mapper.Map<List<VMGastosTablaDinamica>>(await _GastosService.ListGastosForTablaDinamica(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = listUsers.OrderByDescending(_ => _.RegistrationUser).ThenByDescending(_ => _.Gasto).ThenByDescending(_ => _.Tipo_Gasto) });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = e.ToString() });
            }
        }
    }
}
