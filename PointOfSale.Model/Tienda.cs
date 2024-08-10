using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static PointOfSale.Model.Enum;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Model
{
	public partial class Tienda
	{
		public int IdTienda { get; set; }
        public ListaDePrecio? IdListaPrecio { get; set; }
        public string Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public byte[]? Logo { get; set; }        
        public IEnumerable<Turno>? Turnos { get; set; }
        public IEnumerable<User>? Usuarios { get; set; }
        public IEnumerable<Vencimiento>? Vencimientos { get; set; }
        public IEnumerable<Pedido>? Pedidos { get; set; }
        public IEnumerable<Stock>? Stocks { get; set; }
        public IEnumerable<FacturaEmitida>? FacturaEmitidas { get; set; }
        public IEnumerable<Sale>? Sales { get; set; }
        public IEnumerable<ProveedorMovimiento>? ProveedorMovimientos{ get; set; }
        public IEnumerable<Gastos>? Gastos{ get; set; }
        public int? IdAjustes { get; set; }
        public Ajustes? Ajustes { get; set; }
        public int? IdAjustesFacturacion { get; set; }
        public AjustesFacturacion? AjustesFacturacion { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
	