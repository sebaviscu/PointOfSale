using PointOfSale.Business.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Notifications
    {
        public Notifications()
        {

        }

        public Notifications(Product product)
        {
            Descripcion = $"{product.Description} llegó al minimo de stock.";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "/Inventory/Stock";
            Rols = $"{(int)Roles.Administrador},{(int)Roles.Encargado}";
        }

        public Notifications(VentaWeb sale)
        {
            Descripcion = $"Tienes una nueva Venta Web con {sale.DetailSales.Count()} productos.";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "/Shop/VentaWeb";
            Rols = $"{(int)Roles.Administrador},{(int)Roles.Encargado},{(int)Roles.Empleado}";
        }

        public Notifications(Vencimiento vencimiento)
        {
            Descripcion = $"Se ha llegado a la fecha de Vencimiento del Producto {vencimiento.Producto.Description}.";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "/Inventory/Stock";
            Rols = $"{(int)Roles.Administrador},{(int)Roles.Encargado}";
        }

        public Notifications(AjustesFacturacion ajustes)
        {
            Descripcion = $"Está proxima la fecha de caducidad ({ajustes.CertificadoFechaCaducidad.Value.ToShortDateString()}) <br> del certificado ({ajustes.CertificadoNombre}) de AFIP.";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "Ajustes/Index";
            Rols = $"{(int)Roles.Administrador}";
        }

        /// <summary>
        /// Notifiacacion cuando un usuario utiliza el Codigo de Seguridad en una venta
        /// </summary>
        /// <param name="usuario"></param>
        public Notifications(string usuario, string detalle)
        {
            Descripcion = $"El usuario {usuario} ha utilizado el Codigo de Seguridad para {detalle}.";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "";
            Rols = $"{(int)Roles.Administrador},{(int)Roles.Encargado}";
        }

        public Notifications(MovimientoCaja movCaja)
        {
            var tipo =  movCaja.RazonMovimientoCaja != null ? movCaja.RazonMovimientoCaja.Tipo.ToString() : string.Empty;

            Descripcion = $"El usuario {movCaja.RegistrationUser} ha realizado un {tipo} de ${movCaja.Importe}.";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "/MovimientoCaja/Index";
            Rols = $"{(int)Roles.Administrador},{(int)Roles.Encargado}";
        }

        public Notifications(Sale sale, string error)
        {
            Descripcion = $"Ha ocurrido un error al Facturar la venta Nro: {sale.SaleNumber}.<br> Error: {error}";
            IsActive = true;
            RegistrationDate = TimeHelper.GetArgentinaTime();
            Accion = "";
            Rols = $"{(int)Roles.Administrador},{(int)Roles.Encargado},{(int)Roles.Empleado}";
        }

        public int IdNotifications { get; set; }
        public string? Descripcion { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? Accion { get; set; }
        public string Rols { get; set; }
    }
}
