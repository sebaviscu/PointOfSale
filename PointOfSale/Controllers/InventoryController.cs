using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Reportes;
using PointOfSale.Business.Services;
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
        private readonly IMapper _mapper;
        private readonly IImportarExcelService _excelService;
        private readonly ILogger<InventoryController> _logger;
        private readonly ITagService _tagService;
        private readonly IPromocionService _promocionService;

        public InventoryController(ICategoryService categoryService, 
            IProductService productService, 
            IMapper mapper, 
            IImportarExcelService excelService, 
            ILogger<InventoryController> logger, 
            ITagService tagService, 
            IPromocionService promocionService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _mapper = mapper;
            _excelService = excelService;
            _logger = logger;
            _tagService = tagService;
            _promocionService = promocionService;
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
        [HttpGet]
        public async Task<IActionResult> GetCategoriesActive()
        {

            List<VMCategory> vmCategoryList = _mapper.Map<List<VMCategory>>(await _categoryService.ListActive());
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
        /// Selectw de productos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductsActive()
        {
            var gResponse = new GenericResponse<VMProductSimplificado>();
            try
            {
                var productosQuery = await _productService.ListActive();

                return Ok(new { data = _mapper.Map<List<VMProductSimplificado>>(productosQuery) });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al lista de producto active.", _logger, null);
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
        public async Task<IActionResult> CreateProduct([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string vencimientos, [FromForm] string codBarras, [FromForm] string tags, [FromForm] string comodines)
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
                var vmTags = JsonConvert.DeserializeObject<List<VMTag>>(tags);
                var vmComodines = JsonConvert.DeserializeObject<List<VMProductLov>>(comodines);


                var culture = new CultureInfo("es-ES");
                var listPrecios = new List<ListaPrecio>
                {
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_1, decimal.Parse(vmProduct.Price.Replace(".", ","), culture), Convert.ToInt32(vmProduct.PorcentajeProfit)),
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_2, decimal.Parse(vmProduct.Precio2.Replace(".", ","), culture), Convert.ToInt32(vmProduct.PorcentajeProfit2)),
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_3, decimal.Parse(vmProduct.Precio3.Replace(".", ","), culture), Convert.ToInt32(vmProduct.PorcentajeProfit3))
                };

                vmProduct.PriceWeb = vmProduct.PriceWeb.Replace(".", ",");
                vmProduct.PrecioFormatoWeb = vmProduct.PrecioFormatoWeb.Replace(".", ",");

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

                Product product_created = await _productService.Add(
                    prod,
                    listPrecios,
                    _mapper.Map<List<Vencimiento>>(vmListVencimientos),
                    stock,
                    _mapper.Map<List<CodigoBarras>>(vmCodBarras),
                    _mapper.Map<List<Tag>>(vmTags),
                    _mapper.Map<List<ProductLov>>(vmComodines)
                    );

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
        public async Task<IActionResult> EditProduct([FromForm] IFormFile photo, [FromForm] string model, [FromForm] string vencimientos, [FromForm] string codBarras, [FromForm] string tags, [FromForm] string comodines)
        {
            var response = new GenericResponse<VMProduct>();

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
                var vmTags = JsonConvert.DeserializeObject<List<VMTag>>(tags);
                var vmComodines = JsonConvert.DeserializeObject<List<VMProductLov>>(comodines);

                vmProduct.ModificationUser = user.UserName;

                if (photo != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        photo.CopyTo(ms);
                        vmProduct.Photo = ms.ToArray();
                    }
                }

                var culture = new CultureInfo("es-ES");
                var listPrecios = new List<ListaPrecio>
                {
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_1, decimal.Parse(vmProduct.Price.Replace(".", ","), culture), Convert.ToInt32(vmProduct.PorcentajeProfit)),
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_2, decimal.Parse(vmProduct.Precio2.Replace(".", ","), culture), Convert.ToInt32(vmProduct.PorcentajeProfit2)),
                    new ListaPrecio(vmProduct.IdProduct, ListaDePrecio.Lista_3, decimal.Parse(vmProduct.Precio3.Replace(".", ","), culture), Convert.ToInt32(vmProduct.PorcentajeProfit3))
                };

                vmProduct.PriceWeb = vmProduct.PriceWeb.Replace(".", ",");
                vmProduct.PrecioFormatoWeb = vmProduct.PrecioFormatoWeb.Replace(".", ",");

                foreach (var vencimiento in vmListVencimientos.Where(v => v.IdVencimiento == 0))
                {
                    vencimiento.IdProducto = vmProduct.IdProduct;
                    vencimiento.IdTienda = user.IdTienda;
                    vencimiento.RegistrationDate = TimeHelper.GetArgentinaTime();
                    vencimiento.RegistrationUser = user.UserName;
                }

                Stock? stock = null;
                if (vmProduct.Quantity.HasValue && vmProduct.Quantity.Value > 0 && vmProduct.Minimo.HasValue && vmProduct.Minimo.Value >= 0)
                {
                    stock = new Stock(vmProduct.Quantity.Value, (int)vmProduct.Minimo.Value, vmProduct.IdProduct, user.IdTienda);
                }

                var product_edited = await _productService.Edit(
                    _mapper.Map<Product>(vmProduct),
                    listPrecios,
                    _mapper.Map<List<Vencimiento>>(vmListVencimientos),
                    stock,
                    _mapper.Map<List<CodigoBarras>>(vmCodBarras),
                    _mapper.Map<List<Tag>>(vmTags),
                    _mapper.Map<List<ProductLov>>(vmComodines)
                );

                response.State = true;
                response.Object = _mapper.Map<VMProduct>(product_edited);
                return Ok(response);
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

        /// <summary>
        /// Importador de productos por csv.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CargarProductos(IFormFile file)
        {
            ValidarAutorizacion([Roles.Administrador]);

            GenericResponse<List<VMProduct>> gResponse = new GenericResponse<List<VMProduct>>();
            try
            {
                var (exito, products, errores) = await _excelService.ImportarProductoAsync(file, false, false);

                if (errores.Any())
                {
                    gResponse.State = false;
                    gResponse.Message = string.Join("<br>", errores);
                    return StatusCode(StatusCodes.Status400BadRequest, gResponse);
                }

                var vmProduct = _mapper.Map<List<VMProduct>>(products);
                gResponse.State = exito;
                gResponse.Object = vmProduct;
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cargar productos a importar.", _logger, null);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpPost]
        public async Task<IActionResult> ImportarProductos(IFormFile file, bool modificarPrecio, bool productoWeb)
        {
            var gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                var (exito, productos, errores) = await _excelService.ImportarProductoAsync(file, modificarPrecio, productoWeb);

                if (errores.Any())
                {
                    gResponse.State = false;
                    gResponse.Message = string.Join("<br>", errores);
                    return StatusCode(StatusCodes.Status400BadRequest, gResponse);
                }

                var resp = await _productService.Add(productos);

                gResponse.State = resp == string.Empty;
                gResponse.Message = resp;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al importar productos.", _logger, file.FileName);
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


        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] VMTag model)
        {

            var gResponse = new GenericResponse<VMTag>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                Tag category_created = await _tagService.Add(_mapper.Map<Tag>(model));

                model = _mapper.Map<VMTag>(category_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear tag.", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateTag([FromBody] VMTag model)
        {

            var gResponse = new GenericResponse<VMTag>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);

                Tag category_created = await _tagService.Edit(_mapper.Map<Tag>(model));

                model = _mapper.Map<VMTag>(category_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear tag.", _logger, model);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTags()
        {

            var vmCategoryList = _mapper.Map<List<VMTag>>(await _tagService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = vmCategoryList });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTag(int idTag)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _tagService.Delete(idTag);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al borrar Tag.", _logger, idTag);
            }
        }


        public IActionResult Promociones()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recupera promociones para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPromociones()
        {
            var gResponse = new GenericResponse<List<VMPromocion>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.List(user.IdTienda));
                foreach (var p in listPromocion)
                {
                    await SetStringPromocion(p);

                }
                return StatusCode(StatusCodes.Status200OK, new { data = listPromocion });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar promociones", _logger);
            }

        }

        private async Task SetStringPromocion(VMPromocion p)
        {
            var dias = string.Empty;
            var producto = string.Empty;
            var categoria = string.Empty;

            if (p.IdProducto != null)
            {
                var prod = await _productService.Get(Convert.ToInt32(p.IdProducto));
                p.PromocionString += $" [{string.Join(", ", prod.Description)}] ";
                var operador = p.Operador == 0 ? "Igual a" : "Mayor o igual a";
                p.PromocionString += $" [{operador} {p.CantidadProducto} {prod.TipoVenta}] ";
            }

            if (p.IdCategory != null && p.IdCategory.Any())
            {
                var catList = await _categoryService.GetMultiple(p.IdCategory);
                p.PromocionString += " [" + string.Join(", ", catList.Select(_ => _.Description)) + "]";
            }

            if (p.Dias != null && p.Dias.Any())
            {
                var diasList = p.Dias.Select(_ => (Model.Enum.DiasSemana)_).ToList();
                p.PromocionString += " [" + string.Join(", ", diasList.Select(_ => _.ToString())) + "]";
            }
            p.PromocionString += " -> ";
            p.PromocionString += p.Precio != null ? $"Precio fijo: ${p.Precio}" : $"Precio al: {(int)p.Porcentaje}% ";
        }

        [HttpGet]
        public async Task<IActionResult> GetPromocionesActivas()
        {
            var gResponse = new GenericResponse<List<VMPromocion>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                var listPromocion = _mapper.Map<List<VMPromocion>>(await _promocionService.Activas(user.IdTienda));

                gResponse.State = true;
                gResponse.Object = listPromocion;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar promociones activas", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromociones([FromBody] VMPromocion model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.IdTienda = user.IdTienda;
                var usuario_creado = await _promocionService.Add(_mapper.Map<Promocion>(model));

                model = _mapper.Map<VMPromocion>(usuario_creado);

                await SetStringPromocion(model);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear promocion", _logger, model);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdatePromociones([FromBody] VMPromocion vmUser)
        {
            if (!ModelState.IsValid)
            {
                return View(vmUser);
            }


            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var user_edited = await _promocionService.Edit(_mapper.Map<Promocion>(vmUser));

                vmUser = _mapper.Map<VMPromocion>(user_edited);

                await SetStringPromocion(vmUser);
                gResponse.State = true;
                gResponse.Object = vmUser;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar promocion", _logger, vmUser);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeletePromociones(int idPromocion)
        {
            if (!ModelState.IsValid)
            {
                return View(idPromocion);
            }


            var gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                gResponse.State = await _promocionService.Delete(idPromocion);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar promocion", _logger, idPromocion);
            }

        }


        [HttpPut]
        public async Task<IActionResult> CambiarEstadoPromocion(int idPromocion)
        {
            if (!ModelState.IsValid)
            {
                return View(idPromocion);
            }


            var gResponse = new GenericResponse<VMPromocion>();
            try
            {
                var resp = ValidarAutorizacion([Roles.Administrador]);

                var user_edited = await _promocionService.CambiarEstado(idPromocion, resp.UserName);

                var model = _mapper.Map<VMPromocion>(user_edited);
                await SetStringPromocion(model);

                gResponse.Object = model;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cambiar estado de promocion", _logger, idPromocion);
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

        [HttpPost]
        public async Task<IActionResult> CreateVencimiento([FromBody] VMVencimiento model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var gResponse = new GenericResponse<VMVencimiento>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                model.IdTienda = user.IdTienda;
                model.RegistrationUser = user.UserName;
                var usuario_creado = await _productService.AddVencimiento(_mapper.Map<Vencimiento>(model));

                model = _mapper.Map<VMVencimiento>(usuario_creado);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear Vencimiento", _logger, model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVencimientoes([FromBody] VMVencimiento vmUser)
        {
            if (!ModelState.IsValid)
            {
                return View(vmUser);
            }


            var gResponse = new GenericResponse<VMVencimiento>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                var user_edited = await _productService.EditVencimiento(_mapper.Map<Vencimiento>(vmUser));

                vmUser = _mapper.Map<VMVencimiento>(user_edited);

                gResponse.State = true;
                gResponse.Object = vmUser;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar Vencimiento", _logger, vmUser);
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
    }
}