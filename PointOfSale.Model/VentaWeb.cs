using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Business.Utilities;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class VentaWeb
    {
        public VentaWeb()
        {
            DetailSales = new HashSet<DetailSale>();
        }
        public int IdVentaWeb { get; set; }
        public string? SaleNumber { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Comentario { get; set; }
        public int? IdFormaDePago { get; set; }
        public decimal? Total { get; set; } = 0m;
        public decimal? TotalFinal => Total + CostoEnvio - DescuentoRetiroLocal;
        public int? IdTienda { get; set; }
        public EstadoVentaWeb Estado { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public int? IdSale { get; set; }
        public TipoFactura? TipoFactura { get; set; }

        public bool? IsEdit { get; set; }
        public string? EditText { get; set; }

        public decimal? DescuentoRetiroLocal { get; set; } = 0m;
        public string? CruceCallesDireccion { get; set; }
        public decimal? CostoEnvio { get; set; } = 0m;
        public string? ObservacionesUsuario { get; set; } = string.Empty;

        public virtual TypeDocumentSale? FormaDePago { get; set; }
        public virtual ICollection<DetailSale>? DetailSales { get; set; }

        public void SetEditVentaWeb(VentaWeb newVentaWeb)
        {
            IsEdit = true;

            if (!string.IsNullOrEmpty(EditText))
                EditText += "<br>------------------------------------------------------ <br><br>";

            EditText += $"<b>· {newVentaWeb.ModificationUser} {newVentaWeb.ModificationDate} </b> <br>";

            if (Nombre != newVentaWeb.Nombre)
            {
                EditText += $"Nombre: <em>{Nombre}</em> -> {newVentaWeb.Nombre}<br>";
            }
            if (Direccion != newVentaWeb.Direccion)
            {
                EditText += $"Direccion: <em>{Direccion}</em> -> {newVentaWeb.Direccion}<br>";
            }
            if (Telefono != newVentaWeb.Telefono)
            {
                EditText += $"Telefono: <em>{Telefono}</em> -> {newVentaWeb.Telefono}<br>";
            }
            if (IdFormaDePago != newVentaWeb.IdFormaDePago)
            {
                EditText += $"Forma de Pago: <em>{FormaDePago.Description}</em> -> {newVentaWeb.FormaDePago.Description}<br>";
            }
            if (CostoEnvio != newVentaWeb.CostoEnvio)
            {
                EditText += $"Costo Envio: <em>${(int)CostoEnvio}</em> -> ${(int)newVentaWeb.CostoEnvio}<br>";
            }
            if (CruceCallesDireccion != newVentaWeb.CruceCallesDireccion)
            {
                EditText += $"Cruce Calles Direccion: <em>{CruceCallesDireccion}</em> -> {newVentaWeb.CruceCallesDireccion}<br>";
            }
            if (DescuentoRetiroLocal != newVentaWeb.DescuentoRetiroLocal)
            {
                EditText += $"Descuento Retiro Local: <em>${(int)DescuentoRetiroLocal}</em> -> ${(int)newVentaWeb.DescuentoRetiroLocal}<br>";
            }
            if (Comentario != newVentaWeb.Comentario)
            {
                EditText += $"Comentario Cliente: <em>{Comentario}</em> -> {newVentaWeb.Comentario}<br>";
            }
            if (!string.IsNullOrEmpty(ObservacionesUsuario) && ObservacionesUsuario != newVentaWeb.ObservacionesUsuario)
            {
                EditText += $"Obs Usuario: <em>{ObservacionesUsuario}</em> -> {newVentaWeb.ObservacionesUsuario}<br>";
            }
            if (Estado != newVentaWeb.Estado)
            {
                EditText += $"Estado: <em>{Estado.ToString()}</em> -> {newVentaWeb.Estado.ToString()}<br>";
            }
            if (Total != newVentaWeb.Total)
            {
                EditText += $"Total Productos: <em>${(int)Total}</em> -> ${(int)newVentaWeb.Total}<br>";
            }
            EditText += "<br>";
        }


        public void SetEditProductoVentaWeb(string text, List<DetailSale> updatedDetail)
        {
            if (updatedDetail.Any())
            {
                EditText += $"<u>{text}</u>:<br>";

                foreach (DetailSale item in updatedDetail)
                {
                    EditText += $"- {item.DescriptionProduct}: ${(int)item.Price} x {item.Quantity} / {item.TipoVentaString} = ${(int)item.Total}<br>";
                }
                EditText += "<br>";
            }
        }
    }
}
