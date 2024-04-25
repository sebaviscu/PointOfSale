using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Notifications
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        public Notifications()
        {

        }

        public Notifications(Product product)
        {
            var proveedor = product.Proveedor != null ? "(" + product.Proveedor.Nombre + ") " : string.Empty;

            Descripcion = $"{product.Description} {proveedor}llegó al minimo de {product.Minimo} {product.TipoVenta} en stock.";
            IsActive = true;
            RegistrationDate = DateTimeNowArg;
            Accion = "/Inventory/Products";
        }

        public Notifications(VentaWeb sale)
        {
            Descripcion = $"Tienes una nueva Venta Web con {sale.DetailSales.Count()} productos.";
            IsActive = true;
            RegistrationDate = DateTimeNowArg;
            Accion = "/Shop/VentaWeb";
        }

        public Notifications(Vencimiento vencimiento)
        {
            Descripcion = $"Se ha llegado a la fecha de Vencimiento del Producto {vencimiento.Producto.Description}.";
            IsActive = true;
            RegistrationDate = DateTimeNowArg;
            Accion = "/Inventory/Products";
        }

        public int IdNotifications { get; set; }
        public string? Descripcion { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? Accion { get; set; }
    }
}
