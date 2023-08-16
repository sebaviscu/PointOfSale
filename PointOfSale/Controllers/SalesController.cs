using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class SalesController : BaseController
    {
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;
        private readonly IClienteService _clienteService;

        public SalesController(ITypeDocumentSaleService typeDocumentSaleService,
            ISaleService saleService, IMapper mapper, IConverter converter, IClienteService clienteService)
        {
            _typeDocumentSaleService = typeDocumentSaleService;
            _saleService = saleService;
            _mapper = mapper;
            _converter = converter;
            _clienteService = clienteService;
        }
        public IActionResult NewSale()
        {
            return View();
        }

        public IActionResult SalesHistory()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> ListTypeDocumentSale()
        {
            List<VMTypeDocumentSale> vmListTypeDocumentSale = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.GetActive());
            return StatusCode(StatusCodes.Status200OK, vmListTypeDocumentSale);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string search)
        {
            List<VMProduct> vmListProducts = _mapper.Map<List<VMProduct>>(await _saleService.GetProducts(search.Trim()));
            return StatusCode(StatusCodes.Status200OK, vmListProducts);
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes(string search)
        {
            List<VMCliente> vmListClients = _mapper.Map<List<VMCliente>>(await _saleService.GetClients(search));
            foreach (var c in vmListClients)
            {
                c.Total = "$100";
                c.Color = "text-success";
                //c.Color = c.ClienteMovimientos.Sum(x => x.Total) > 0 ? "text-success" : "text-danger";
            }
            return StatusCode(StatusCodes.Status200OK, vmListClients);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] VMSale model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });

            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;

                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

                string idTurno = claimuser.Claims.Where(c => c.Type == "Turno").Select(c => c.Value).SingleOrDefault();

                model.IdUsers = int.Parse(idUsuario);
                model.IdTurno = int.Parse(idTurno);
                model.IdTienda= user.IdTienda;

                Sale sale_created = await _saleService.Register(_mapper.Map<Sale>(model));

                if (model.ClientId.HasValue)
                {
                    var mov = await _clienteService.RegistrarMovimiento(
                        model.ClientId.Value,
                        decimal.Parse(model.Total.Replace('.', ',')),
                        user.UserName,
                        sale_created.IdSale,
                        model.TipoMovimiento.Value);

                    sale_created.IdClienteMovimiento = mov.IdClienteMovimiento;
                    sale_created = await _saleService.Edit(sale_created);
                }

                model = _mapper.Map<VMSale>(sale_created);

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


        [HttpGet]
        public async Task<IActionResult> History(string saleNumber, string startDate, string endDate)
        {

            List<VMSale> vmHistorySale = _mapper.Map<List<VMSale>>(await _saleService.SaleHistory(saleNumber, startDate, endDate));
            return StatusCode(StatusCodes.Status200OK, vmHistorySale);
        }

        public IActionResult ShowPDFSale(string saleNumber)
        {
            string urlTemplateView = $"{this.Request.Scheme}://{this.Request.Host}/Template/PDFSale?saleNumber={saleNumber}";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait
                },
                Objects = {
                    new ObjectSettings(){
                        Page = urlTemplateView
                    }
                }
            };
            var archivoPDF = _converter.Convert(pdf);
            return File(archivoPDF, "application/pdf");
        }

    }
}
