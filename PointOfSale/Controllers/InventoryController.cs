using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
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
		public InventoryController(ICategoryService categoryService, IProductService productService, IMapper mapper, ISaleService saleService)
		{
			_categoryService = categoryService;
			_productService = productService;
			_mapper = mapper;
            _saleService = saleService;
        }

		public IActionResult Categories()
		{
			return View();
		}

		public IActionResult Products()
		{
			return View();
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

				Category edited_category = await _categoryService.Edit(_mapper.Map<Category>(model));
				edited_category.ModificationUser = user.UserName;

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
			var s = await _productService.List();
			List<VMProduct> vmProductList = _mapper.Map<List<VMProduct>>(await _productService.List());
			return StatusCode(StatusCodes.Status200OK, new { data = vmProductList });
		}

		[HttpPost]
		public async Task<IActionResult> CreateProduct([FromForm] IFormFile photo, [FromForm] string model)
		{
			GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
			try
			{
				VMProduct vmProduct = JsonConvert.DeserializeObject<VMProduct>(model);

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

				Product product_created = await _productService.Add(_mapper.Map<Product>(vmProduct));

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
		public async Task<IActionResult> EditProduct([FromForm] IFormFile photo, [FromForm] string model)
		{
			var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

			GenericResponse<VMProduct> gResponse = new GenericResponse<VMProduct>();
			try
			{
				VMProduct vmProduct = JsonConvert.DeserializeObject<VMProduct>(model);
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

				Product product_edited = await _productService.Edit(_mapper.Map<Product>(vmProduct));

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
				var resp = await _productService.EditMassive(user.UserName, data);

				gResponse.State = resp;
			}
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
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
    }
}
