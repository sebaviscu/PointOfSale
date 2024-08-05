using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class Cliente
    {
        public int IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Cuil { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public IEnumerable<ClienteMovimiento>? ClienteMovimientos { get; set; }
        public int IdTienda { get; set; }
        public CondicionIva? CondicionIva { get; set; }
        public string? Comentario { get; set; }
        public bool IsActive { get; set; }
        public int? IdFacturaEmitida { get; set; }
        public virtual FacturaEmitida FacturaEmitida { get; set; }
    }
}
