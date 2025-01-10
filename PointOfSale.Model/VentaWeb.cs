using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class VentaWeb
    {
        public VentaWeb()
        {
            DetailSales = new HashSet<DetailSale>();
        }
        public int IdVentaWeb{ get; set; }
        public string? SaleNumber { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Comentario { get; set; }
        public int? IdFormaDePago { get; set; }
        public decimal? Total { get; set; }
        public int? IdTienda { get; set; }
        public EstadoVentaWeb Estado { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public int? IdSale { get; set; }

        public bool? IsEdit { get; set; }
        public string? EditText { get; set; }

        public decimal? DescuentoRetiroLocal { get; set; }
        public string? CruceCallesDireccion { get; set; }
        public decimal? CostoEnvio { get; set; }
        public string? ObservacionesUsuario { get; set; }

        public virtual TypeDocumentSale? FormaDePago { get; set; }
        public virtual ICollection<DetailSale>? DetailSales { get; set; }


        public void SetEditVentaWeb(string user, DateTime Date)
        {
            var text = string.Empty;


            text += $"{user} {Date} <br>";
            text += $"Cliente: {Nombre}<br>";
            text += $"Dir: {Direccion} | Tel: {Telefono} \n";
            text += $"Total: ${Total} | {FormaDePago.Description} <br><br>";

            foreach (var e in DetailSales)
            {
                text += $"- {e.DescriptionProduct}: ${e.Price} x {e.Quantity} / {e.TipoVentaString} = ${e.Total}<br>";
            }

            if (!string.IsNullOrEmpty(EditText))
            {
                text += "------------------------------------------------------ <br>";
            }
            text += EditText;
            EditText = text;
        }
    }
}
