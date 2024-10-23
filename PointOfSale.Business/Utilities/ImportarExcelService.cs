
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using System.IO;
using System.Text.RegularExpressions;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Utilities
{
    public class ImportarExcelService : IImportarExcelService
    {

        private readonly IProveedorService _proveedorService;
        private readonly ICategoryService _categoryService;

        public ImportarExcelService(IProveedorService proveedorService, ICategoryService categoryService)
        {
            _proveedorService = proveedorService;
            _categoryService = categoryService;
        }


        public async Task<(bool exito, List<Product>? productos, List<string> errores)> ImportarProductoAsync(IFormFile file, bool modificarPrecio, bool productoWeb)
        {
            // Validar si el archivo es válido
            if (file == null || file.Length == 0)
            {
                return (false, null, new List<string> { "El archivo está vacío o no es válido." });
            }

            List<Product> products = new List<Product>();
            List<string> errores = new List<string>();

            var listaProveedores = await _proveedorService.List();
            var listaCategorias = await _categoryService.List();

            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int rowIndex = 0; // Para identificar el número de fila
                    while (reader.Read())
                    {
                        rowIndex++;
                        try
                        {
                            // Omitir encabezados
                            if (reader.GetValue(0)?.ToString() == "Descripcion*" || ( string.IsNullOrEmpty(reader.GetValue(0)?.ToString()) && string.IsNullOrEmpty(reader.GetValue(2)?.ToString()) && string.IsNullOrEmpty(reader.GetValue(5)?.ToString())))
                                continue;

                            if(string.IsNullOrEmpty(reader.GetValue(0)?.ToString()) || 
                                string.IsNullOrEmpty(reader.GetValue(2)?.ToString())|| 
                                string.IsNullOrEmpty(reader.GetValue(5)?.ToString()))
                            {
                                throw new FormatException("Valores obligatorios no completados.");
                            }

                            if (reader.GetValue(2)?.ToString().ToLower() != "kg" && reader.GetValue(2)?.ToString().ToLower() != "u")
                                throw new Exception("Tipo de venta inválido");

                            Product product = ParseProduct(reader, listaProveedores, listaCategorias, modificarPrecio, productoWeb);
                            products.Add(product);
                        }
                        catch (Exception ex)
                        {
                            // Registrar el error con el número de fila
                            errores.Add($"Error en la fila {rowIndex}: {ex.Message}");
                        }
                    }
                }
            }

            bool exito = errores.Count == 0;
            return (exito, products, errores);
        }

        private Product ParseProduct(IExcelDataReader reader, List<Proveedor> listaProveedores, List<Category> listaCategorias, bool modificarPrecio, bool productoWeb)
        {

            var proveedor = listaProveedores.FirstOrDefault(_ => _.Nombre.ToLower() == reader.GetValue(12)?.ToString()?.ToLower());
            if(proveedor == null && !string.IsNullOrEmpty(reader.GetValue(12)?.ToString()?.ToLower()))
            {
                throw new Exception($"Proveedor '{reader.GetValue(12)?.ToString()}' no encontrado");
            }

            var categoria = listaCategorias.FirstOrDefault(_ => _.Description.ToLower() == reader.GetValue(13)?.ToString()?.ToLower());
            if(categoria == null && !string.IsNullOrEmpty(reader.GetValue(13)?.ToString()?.ToLower()))
            {
                throw new Exception($"Categoria '{reader.GetValue(13)?.ToString()}' no encontrada");
            }

            var tipoVenta = reader.GetValue(2)?.ToString().ToLower() == "kg" ? TipoVenta.Kg : TipoVenta.U;
            var precioWeb = ParseDecimal(reader.GetValue(11), "Precio Web");

            Product product = new Product
            {
                IdProduct = 0,
                IsActive = true,
                Description = reader.GetValue(0)?.ToString(),
                TipoVenta = tipoVenta,
                CostPrice = ParseDecimal(reader.GetValue(3), "Costo"),
                ListaPrecios = ParseListaPrecios(reader),
                PriceWeb = precioWeb,
                Proveedor = proveedor,
                IdProveedor = proveedor?.IdProveedor,
                IdCategory = categoria?.IdCategory,
                IdCategoryNavigation = categoria,
                Destacado = false,
                ProductoWeb = productoWeb,
                Iva = ParseDecimal(reader.GetValue(4), "IVA"),
                Comentario = string.Empty,
                FormatoWeb = tipoVenta == TipoVenta.Kg ? 1000 : 1,
                PrecioFormatoWeb = precioWeb,
                ModificarPrecio = modificarPrecio,
                PrecioAlMomento = false,
                ExcluirPromociones = false
            };

            var codBarras = reader.GetValue(1)?.ToString();
            if (!string.IsNullOrEmpty(codBarras))
            {
                product.CodigoBarras = new List<CodigoBarras>() { new CodigoBarras(ParseInt(codBarras, "Codigo de barras").ToString()) };
            }

            return product;
        }

        private List<ListaPrecio> ParseListaPrecios(IExcelDataReader reader)
        {
            var listaPrecios = new List<ListaPrecio>
    {
        new ListaPrecio(ListaDePrecio.Lista_1)
        {
            PorcentajeProfit = ParseInt(reader.GetValue(5), "Procentaje de ganancia 1"),
            Precio = ParseDecimal(reader.GetValue(6), "Precio 1")
        },
        new ListaPrecio(ListaDePrecio.Lista_2)
        {
            PorcentajeProfit = ParseInt(reader.GetValue(7), "Procentaje de ganancia 2"),
            Precio = ParseDecimal(reader.GetValue(8), "Precio 2")
        },
        new ListaPrecio(ListaDePrecio.Lista_3)
        {
            PorcentajeProfit = ParseInt(reader.GetValue(9), "Procentaje de ganancia 3"),
            Precio = ParseDecimal(reader.GetValue(10), "Precio 3")
        }
    };

            return listaPrecios;
        }

        private decimal ParseDecimal(object value, string detalle)
        {
            if (value == null)
                return 0;

            try
            {
                if (value != null && decimal.TryParse(value.ToString(), out decimal result))
                {
                    return result;
                }
                throw new FormatException();
            }
            catch (FormatException ex)
            {
                throw new FormatException($"El valor '{value}' no es un decimal válido para {detalle}.");
            }
        }

        private int ParseInt(object value, string detalle)
        {
            if (value == null)
                return 0;

            try
            {
                if (value != null && int.TryParse(value.ToString(), out int result))
                {
                    return result;
                }
                throw new FormatException();
            }
            catch (Exception ex)
            {
                throw new FormatException($"El valor '{value}' no es un entero válido para {detalle}.");
            }
        }

    }
}
