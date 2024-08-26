using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Reportes;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Globalization;
using static PointOfSale.Model.Enum;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class InventoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IImportarExcelService _excelService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(ICategoryService categoryService, IProductService productService, IMapper mapper, ISaleService saleService, IWebHostEnvironment env, IImportarExcelService excelService, ILogger<InventoryController> logger)
        {
            _categoryService = categoryService;
            _productService = productService;
            _mapper = mapper;
            _saleService = saleService;
            _env = env;
            _excelService = excelService;
            _logger = logger;
        }

        public IActionResult Categories()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
        }

        public IActionResult Products()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
        }
        public IActionResult Stock()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {

            List<VMCategory> vmCategoryList = _mapper.Map<List<VMCategory>>(await _categoryService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = vmCategoryList });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] VMCategory model)
        {

            GenericResponse<VMCategory> gResponse = new GenericResponse<VMCategory>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                Category category_created = await _categoryService.Add(_mapper.Map<Category>(model));

                model = _mapper.Map<VMCategory>(category_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear categoria.", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] VMCategory model)
        {

            GenericResponse<VMCategory> gResponse = new GenericResponse<VMCategory>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.ModificationUser = user.UserName;
                Category edited_category = await _categoryService.Edit(_mapper.Map<Category>(model));

                model = _mapper.Map<VMCategory>(edited_category);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar categoria.", _logger, model);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int idCategory)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _categoryService.Delete(idCategory);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al borrar categoria.", _logger, idCategory);
            }
        }

        /// <summary>
        /// Recupera stock para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetStocks()
        {
            var gResponse = new GenericResponse<List<VMStock>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                List<VMStock> vmCategoryList = _mapper.Map<List<VMStock>>(await _productService.ListStock(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = vmCategoryList });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al lista de stock.", _logger, null);
            }
        }

        /// <summary>
        /// Tabla de productos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var gResponse = new GenericResponse<VMProductSimplificado>();
            try
            {
                var productosQuery = await _productService.List();

                return Ok(new { data = _mapper.Map<List<VMProductSimplificado>>(productosQuery) });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al lista de producto.", _logger, null);
            }
        }

        /// <summary>
        /// Recupera un producto cuando se abre el modal de edit.
        /// </summary>
        /// <param name="idProduct"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProduct(int idProduct)
        {

            var gResponse = new GenericResponse<VMProduct>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var producto = await _productService.Get(idProduct);
                var stock = await _productService.GetStockByIdProductIdTienda(idProduct, user.IdTienda);

                var prod = _mapper.Map<VMProduct>(producto);
                if (stock != null)
                {
                    prod.Minimo = stock.StockMinimo;
                    prod.Quantity = stock.StockActual;
                }

                gResponse.State = true;
                gResponse.Object = prod;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar producto.", _logger, idProduct);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string vencimientos, [FromForm] string codBarras)
        {

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var settings = new JsonSerializerSettings
                {
                    DateFormatString = "dd/MM/yyyy",
                    Culture = CultureInfo.InvariantCulture
                };

                var vmProduct = JsonConvert.DeserializeObject<VMProduct>(model);
                var vmListVencimientos = JsonConvert.DeserializeObject<List<VMVencimiento>>(vencimientos, settings);
                var vmCodBarras = JsonConvert.DeserializeObject<List<VMCodigoBarras>>(codBarras);


                var listPrecios = new List<ListaPrecio>()
                {
                    new ListaPrecio(0, ListaDePrecio.Lista_1, Convert.ToDecimal(vmProduct.Price), Convert.ToInt32(vmProduct.PorcentajeProfit)),
                    new ListaPrecio(0, ListaDePrecio.Lista_2, Convert.ToDecimal(vmProduct.Precio2), Convert.ToInt32(vmProduct.PorcentajeProfit2)),
                    new ListaPrecio(0, ListaDePrecio.Lista_3, Convert.ToDecimal(vmProduct.Precio3), Convert.ToInt32(vmProduct.PorcentajeProfit3))
                };

                if (photo != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        photo.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        vmProduct.Photo = fileBytes;
                    }
                }
                else
                    vmProduct.Photo = null;

                var prod = _mapper.Map<Product>(vmProduct);

                foreach (var v in vmListVencimientos)
                {
                    v.IdTienda = user.IdTienda;
                    v.RegistrationDate = TimeHelper.GetArgentinaTime();
                    v.RegistrationUser = user.UserName;
                }

                Stock stock = null;
                if (vmProduct.Quantity.HasValue && vmProduct.Quantity.Value > 0 && vmProduct.Minimo.HasValue && vmProduct.Minimo.Value >= 0)
                {
                    stock = new Stock(vmProduct.Quantity.Value, (int)vmProduct.Minimo.Value, 0, user.IdTienda);
                }

                Product product_created = await _productService.Add(prod, listPrecios, _mapper.Map<List<Vencimiento>>(vmListVencimientos), stock, _mapper.Map<List<CodigoBarras>>(vmCodBarras));

                vmProduct = _mapper.Map<VMProduct>(product_created);

                gResponse.State = true;
                gResponse.Object = vmProduct;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear producto.", _logger, model: null, ("model", model), ("vencimientos", vencimientos));
            }

        }

        [HttpPut]
        public async Task<IActionResult> EditProduct([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string vencimientos, [FromForm] string codBarras)
        {

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var settings = new JsonSerializerSettings
                {
                    DateFormatString = "dd/MM/yyyy",
                    Culture = CultureInfo.InvariantCulture
                };

                VMProduct vmProduct = JsonConvert.DeserializeObject<VMProduct>(model);
                var vmListVencimientos = JsonConvert.DeserializeObject<List<VMVencimiento>>(vencimientos, settings);
                var vmCodBarras = JsonConvert.DeserializeObject<List<VMCodigoBarras>>(codBarras);

                vmProduct.ModificationUser = user.UserName;

                if (photo != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        photo.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        vmProduct.Photo = fileBytes;
                    }
                }
                else
                    vmProduct.Photo = null;

                var listPrecios = new List<ListaPrecio>()
                {
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_1, Convert.ToDecimal(vmProduct.Price), Convert.ToInt32(vmProduct.PorcentajeProfit)),
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_2, Convert.ToDecimal(vmProduct.Precio2),Convert.ToInt32(vmProduct.PorcentajeProfit2)),
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_3, Convert.ToDecimal(vmProduct.Precio3),Convert.ToInt32(vmProduct.PorcentajeProfit3))
                };

                foreach (var v in vmListVencimientos.Where(_ => _.IdVencimiento == 0))
                {
                    v.IdProducto = vmProduct.IdProduct;
                    v.IdTienda = user.IdTienda;
                    v.RegistrationDate = TimeHelper.GetArgentinaTime();
                    v.RegistrationUser = user.UserName;
                }

                Stock stock = null;
                if (vmProduct.Quantity.HasValue && vmProduct.Quantity.Value > 0 && vmProduct.Minimo.HasValue && vmProduct.Minimo.Value >= 0)
                {
                    stock = new Stock(vmProduct.Quantity.Value, (int)vmProduct.Minimo.Value, vmProduct.IdProduct, user.IdTienda);
                }

                Product product_edited = await _productService.Edit(_mapper.Map<Product>(vmProduct), listPrecios, _mapper.Map<List<Vencimiento>>(vmListVencimientos), stock, _mapper.Map<List<CodigoBarras>>(vmCodBarras));

                vmProduct = _mapper.Map<VMProduct>(product_edited);

                gResponse.State = true;
                gResponse.Object = vmProduct;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al editar producto.", _logger, model: null, ("model", model), ("vencimientos", vencimientos));
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditMassiveProducts([FromBody] EditeMassiveProducts data)
        {

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var listPrecios = new List<ListaPrecio>()
                {
                    new ListaPrecio(0, ListaDePrecio.Lista_1, Convert.ToDecimal(data.Precio), Convert.ToInt32(data.Profit)),
                    new ListaPrecio(0, ListaDePrecio.Lista_2, Convert.ToDecimal(data.Precio2),Convert.ToInt32(data.PorcentajeProfit2)),
                    new ListaPrecio(0, ListaDePrecio.Lista_3, Convert.ToDecimal(data.Precio3),Convert.ToInt32(data.PorcentajeProfit3))
                };

                var resp = await _productService.EditMassive(user.UserName, data, listPrecios);

                gResponse.State = resp;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error en edicion masiva de productos.", _logger, data);
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditMassiveProductsForTable([FromBody] List<EditeMassiveProductsTable> data)
        {

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _productService.EditMassivePorTabla(user.UserName, data);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error en edicion masiva de productos por tabla.", _logger, data);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int IdProduct)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _productService.Delete(IdProduct);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al borrar producto.", _logger, IdProduct);
            }

        }

        //[HttpGet]
        //public async Task<IActionResult> GetProductsSearch(string search)
        //{
        //    List<VMProduct> vmListProducts = _mapper.Map<List<VMProduct>>(await _saleService.GetProducts(search.Trim()));
        //    return StatusCode(StatusCodes.Status200OK, vmListProducts);
        //}


        [HttpGet]
        public async Task<IActionResult> GetCategoriesSearch(string search)
        {
            List<VMCategory> vmListCategories = _mapper.Map<List<VMCategory>>(await _categoryService.GetCategoriesSearch(search.Trim()));
            return StatusCode(StatusCodes.Status200OK, vmListCategories);
        }


        [HttpPost]
        public async Task<IActionResult> ImprimirListaPrecios([FromBody] VMImprimirPrecios model)
        {
            var gResponse = new GenericResponse<byte[]>();
            try
            {
                var listtaProds = await _productService.GetProductsByIdsActive(model.IdProductos, model.ListaPrecio);

                gResponse.Object = ListaPreciosImprimir.Imprimir(listtaProds, model.CodigoBarras, model.FechaModificacion);
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al imprimir lista de precios de productos.", _logger, model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetVencimientos()
        {
            var gResponse = new GenericResponse<List<VMVencimiento>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                List<VMVencimiento> vmVencimientos = _mapper.Map<List<VMVencimiento>>(await _productService.GetProximosVencimientos(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = vmVencimientos.OrderBy(_ => _.Estado).ThenBy(_ => _.FechaVencimiento) });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de vencimientos.", _logger, null);
            }

        }

        [HttpGet]
        public async Task<IActionResult> ImportarProductos(string path)
        {

            GenericResponse<List<VMProduct>> gResponse = new GenericResponse<List<VMProduct>>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var (exito, products) = await _excelService.ImportarProductoAsync(path);

                foreach (var p in products)
                {
                    try
                    {
                        Product product_created = await _productService.Add(p);
                    }
                    catch (Exception e)
                    {
                        exito = false;
                        gResponse.Message = e.Message;
                        throw;
                    }
                }

                gResponse.State = exito;
                return StatusCode(StatusCodes.Status200OK, gResponse);

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al importar productos.", _logger, path);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteVencimiento(int idVencimiento)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _productService.DeleteVencimiento(idVencimiento);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar vencimientos.", _logger, idVencimiento);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCodigoBarras(int idCodigoBarras)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _productService.DeleteCodigoBarras(idCodigoBarras);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar codigo de barras.", _logger, idCodigoBarras);
            }
        }

    }
}
