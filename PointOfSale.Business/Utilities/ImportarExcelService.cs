
using ExcelDataReader;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
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

        public async Task<(bool, List<Product>?)> ImportarProductoAsync(string filePath)
        {
            string patron = @"(\.xlsx|\.csv)$";
            Regex regex = new Regex(patron);

            if (string.IsNullOrEmpty(filePath) || !regex.IsMatch(filePath))
            {
                return (false, null);
            }

            List<Product> products = new List<Product>();

            var listaProveedores = await _proveedorService.List();
            var listaCategorias = await _categoryService.List();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        Product product = new Product();
                        product.IdProduct = 0;
                        product.IsActive = true;

                        var codBarras = reader.GetValue(0)?.ToString();
                        if(!string.IsNullOrEmpty(codBarras))
                        {
                            product.CodigoBarras = new List<CodigoBarras>() { new CodigoBarras(codBarras) };
                        }

                        product.Description = reader.GetValue(1)?.ToString();
                        product.TipoVenta = reader.GetValue(2)?.ToString() == "Kg" ? Model.Enum.TipoVenta.Kg : Model.Enum.TipoVenta.U;

                        if (!string.IsNullOrEmpty(reader.GetValue(3)?.ToString()) && decimal.TryParse(reader.GetValue(3)?.ToString(), out decimal costo))
                        {
                            product.CostPrice = costo;
                        }

                        var listaPrecios = new List<ListaPrecio>();
                        var listaPrecios1 = new ListaPrecio(ListaDePrecio.Lista_1);
                        var listaPrecios2 = new ListaPrecio(ListaDePrecio.Lista_2);
                        var listaPrecios3 = new ListaPrecio(ListaDePrecio.Lista_3);

                        if (!string.IsNullOrEmpty(reader.GetValue(4)?.ToString()) && int.TryParse(reader.GetValue(4)?.ToString(), out int ganancia1))
                        {
                            product.PorcentajeProfit = ganancia1;
                            listaPrecios1.PorcentajeProfit = ganancia1;
                        }
                        else
                        {
                            product.PorcentajeProfit = 0;
                            listaPrecios1.PorcentajeProfit = 0;
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(5)?.ToString()) && decimal.TryParse(reader.GetValue(5)?.ToString(), out decimal precio1))
                        {
                            product.Price = precio1;
                            listaPrecios1.Precio = precio1;
                        }
                        else
                        {
                            product.Price = 0;
                            listaPrecios1.Precio = 0;
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(6)?.ToString()) && int.TryParse(reader.GetValue(6)?.ToString(), out int ganancia2))
                        {
                            listaPrecios2.PorcentajeProfit = ganancia2;
                        }
                        else
                        {
                            listaPrecios2.PorcentajeProfit = 0;
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(7)?.ToString()) && decimal.TryParse(reader.GetValue(7)?.ToString(), out decimal precio2))
                        {
                            listaPrecios2.Precio = precio2;
                        }
                        else
                        {
                            listaPrecios2.Precio = 0;
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(8)?.ToString()) && int.TryParse(reader.GetValue(8)?.ToString(), out int ganancia3))
                        {
                            listaPrecios3.PorcentajeProfit = ganancia3;
                        }
                        else
                        {
                            listaPrecios2.PorcentajeProfit = 0;
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(9)?.ToString()) && decimal.TryParse(reader.GetValue(9)?.ToString(), out decimal precio3))
                        {
                            listaPrecios3.Precio = precio3;
                        }
                        else
                        {
                            listaPrecios2.Precio = 0;
                        }

                        listaPrecios.Add(listaPrecios1);
                        listaPrecios.Add(listaPrecios2);
                        listaPrecios.Add(listaPrecios3);
                        product.ListaPrecios = listaPrecios;

                        if (!string.IsNullOrEmpty(reader.GetValue(10)?.ToString()) && decimal.TryParse(reader.GetValue(10)?.ToString(), out decimal priceWeb))
                        {
                            product.PriceWeb = priceWeb;
                        }
                        else
                        {
                            product.PriceWeb = 0;
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(11)?.ToString()))
                        {
                            var proveedor = listaProveedores.FirstOrDefault(_ => _.Nombre == reader.GetValue(11)?.ToString());
                            if (proveedor != null)
                            {
                                product.IdProveedor = proveedor.IdProveedor;
                                product.Proveedor = proveedor;
                            }
                        }

                        if (!string.IsNullOrEmpty(reader.GetValue(12)?.ToString()))
                        {
                            var categoria = listaCategorias.FirstOrDefault(_ => _.Description == reader.GetValue(12)?.ToString());
                            if (categoria != null)
                            {
                                product.IdCategory = categoria.IdCategory;
                                product.IdCategoryNavigation = categoria;
                            }
                        }

                        products.Add(product);
                    }
                }
            }

            return (true, products);
        }
    }
}
