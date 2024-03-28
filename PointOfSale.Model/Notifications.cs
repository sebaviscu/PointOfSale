using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Notifications
    {
        public Notifications()
        {

        }
        public Notifications(Product product)
        {
            Descripcion = $"{product.Description} llegó al minimo de {product.Minimo} {product.TipoVenta} en stock.";
            IsActive = true;
            RegistrationDate = DateTime.Now;
        }

        public Notifications(VentaWeb sale)
        {
            Descripcion = $"Tienes una nueva Venta Web con {sale.DetailSales.Count()} productos.";
            IsActive = true;
            RegistrationDate = DateTime.Now;
        }
        public Notifications(Vencimiento vencimiento)
        {
            Descripcion = $"Se ha llegado a la fecha de Vencimiento del Producto {vencimiento.Producto.Description}.";
            IsActive = true;
            RegistrationDate = DateTime.Now;
        }

        public int IdNotifications { get; set; }
        public string? Descripcion { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
