using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Reportes;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Globalization;
using static PointOfSale.Model.Enum;

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

        public InventoryController(ICategoryService categoryService, IProductService productService, IMapper mapper, ISaleService saleService, IWebHostEnvironment env, IImportarExcelService excelService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _mapper = mapper;
            _saleService = saleService;
            _env = env;
            _excelService = excelService;
        }

        public IActionResult Categories()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });
            return ValidateSesionViewOrLogin();
        }

        public IActionResult Products()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });
            return ValidateSesionViewOrLogin();
        }
        public IActionResult Stock()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });
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
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMCategory> gResponse = new GenericResponse<VMCategory>();
            try
            {
                Category category_created = await _categoryService.Add(_mapper.Map<Category>(model));

                model = _mapper.Map<VMCategory>(category_created);

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
        public async Task<IActionResult> UpdateCategory([FromBody] VMCategory model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMCategory> gResponse = new GenericResponse<VMCategory>();
            try
            {

                model.ModificationUser = user.UserName;
                Category edited_category = await _categoryService.Edit(_mapper.Map<Category>(model));

                model = _mapper.Map<VMCategory>(edited_category);

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
        public async Task<IActionResult> DeleteCategory(int idCategory)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _categoryService.Delete(idCategory);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var productos = await _productService.List();

                // Asignar Photo a null y mapear a VMProduct en una sola operación
                var vmProductList = productos.Select(producto =>
                {
                    producto.Photo = null;
                    return _mapper.Map<VMProduct>(producto);
                }).ToList();

                return Ok(new { data = vmProductList });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Se produjo un error al obtener los productos. Error: " + ex.Message });
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
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            try
            {
                var producto = await _productService.Get(idProduct);
                var stock = await _productService.GetStockByIdProductIdTienda(idProduct, user.IdTienda);

                var prod = _mapper.Map<VMProduct>(producto);
                if (stock != null)
                {
                    prod.Minimo = stock.StockMinimo;
                    prod.Quantity = stock.StockActual;
                }

                return Ok(new { data = prod });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Se produjo un error al obtener los productos. Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string vencimientos)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var settings = new JsonSerializerSettings
                {
                    DateFormatString = "dd/MM/yyyy",
                    Culture = CultureInfo.InvariantCulture
                };

                var vmProduct = JsonConvert.DeserializeObject<VMProduct>(model);
                var vmListVencimientos = JsonConvert.DeserializeObject<List<VMVencimiento>>(vencimientos, settings);


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
                    stock = new Stock(
                    vmProduct.Quantity.Value,
                    (int)vmProduct.Minimo.Value,
                    0,
                    user.IdTienda);
                }

                Product product_created = await _productService.Add(prod, listPrecios, _mapper.Map<List<Vencimiento>>(vmListVencimientos), stock);

                vmProduct = _mapper.Map<VMProduct>(product_created);

                gResponse.State = true;
                gResponse.Object = vmProduct;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> EditProduct([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string vencimientos)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var settings = new JsonSerializerSettings
                {
                    DateFormatString = "dd/MM/yyyy",
                    Culture = CultureInfo.InvariantCulture
                };

                VMProduct vmProduct = JsonConvert.DeserializeObject<VMProduct>(model);
                var vmListVencimientos = JsonConvert.DeserializeObject<List<VMVencimiento>>(vencimientos, settings);

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
                    stock = new Stock(
                    vmProduct.Quantity.Value,
                    (int)vmProduct.Minimo.Value,
                    vmProduct.IdProduct,
                    user.IdTienda);
                }

                Product product_edited = await _productService.Edit(_mapper.Map<Product>(vmProduct), listPrecios, _mapper.Map<List<Vencimiento>>(vmListVencimientos), stock);

                vmProduct = _mapper.Map<VMProduct>(product_edited);

                gResponse.State = true;
                gResponse.Object = vmProduct;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> EditMassiveProducts([FromBody] EditeMassiveProducts data)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {

                var listPrecios = new List<ListaPrecio>()
                {
                    new ListaPrecio(0, ListaDePrecio.Lista_1, Convert.ToDecimal(data.Precio), Convert.ToInt32(data.Profit)),
                    new ListaPrecio(0, ListaDePrecio.Lista_2, Convert.ToDecimal(data.Precio2),Convert.ToInt32(data.PorcentajeProfit2)),
                    new ListaPrecio(0, ListaDePrecio.Lista_3, Convert.ToDecimal(data.Precio3),Convert.ToInt32(data.PorcentajeProfit3))
                };

                var resp = await _productService.EditMassive(user.UserName, data, listPrecios);

                gResponse.State = resp;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.ToString();
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> EditMassiveProductsForTable([FromBody] List<EditeMassiveProductsTable> data)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
            try
            {
                var resp = await _productService.EditMassivePorTabla(user.UserName, data);

                gResponse.State = true;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.ToString();
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int IdProduct)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _productService.Delete(IdProduct);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsSearch(string search)
        {
            List<VMProduct> vmListProducts = _mapper.Map<List<VMProduct>>(await _saleService.GetProducts(search.Trim()));
            return StatusCode(StatusCodes.Status200OK, vmListProducts);
        }


        [HttpGet]
        public async Task<IActionResult> GetCategoriesSearch(string search)
        {
            List<VMCategory> vmListCategories = _mapper.Map<List<VMCategory>>(await _categoryService.GetCategoriesSearch(search.Trim()));
            return StatusCode(StatusCodes.Status200OK, vmListCategories);
        }


        [HttpPost]
        public async Task<IActionResult> ImprimirListaPrecios([FromBody] VMImprimirPrecios model)
        {
            try
            {
                var listtaProds = await _productService.GetProductsByIdsActive(model.IdProductos, model.ListaPrecio);

                var doc = ListaPreciosImprimir.Imprimir(listtaProds, model.CodigoBarras, model.FechaModificacion);

                return StatusCode(StatusCodes.Status200OK, new { state = true, data = doc });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status200OK, new { state = false, error = e.ToString() });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetVencimientos()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });

            List<VMVencimiento> vmVencimientos = _mapper.Map<List<VMVencimiento>>(await _productService.GetProximosVencimientos(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = vmVencimientos.OrderBy(_ => _.Estado).ThenBy(_ => _.FechaVencimiento) });
        }

        [HttpGet]
        public async Task<IActionResult> CargarProductosAsync(string path)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<List<VMProduct>> gResponse = new GenericResponse<List<VMProduct>>();
            try
            {
                var (exito, products) = await _excelService.ImportarProductoAsync(path);

                var vmProduct = _mapper.Map<List<VMProduct>>(products);

                gResponse.State = exito;
                gResponse.Object = vmProduct;

            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> ImportarProductosAsync(string path)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<List<VMProduct>> gResponse = new GenericResponse<List<VMProduct>>();
            try
            {
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

            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteVencimiento(int idVencimiento)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.State = await _productService.DeleteVencimiento(idVencimiento);
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
