using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
	public partial class Tienda
	{
		public int IdTienda { get; set; }
        public ListaDePrecio? IdListaPrecio { get; set; }
        public string Nombre { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? NombreImpresora { get; set; }
        public byte[]? Logo { get; set; }        
        public bool? Principal { get; set; }
        public IEnumerable<Turno>? Turnos { get; set; }
        public IEnumerable<User>? Usuarios { get; set; }
        public IEnumerable<Vencimiento>? Vencimientos { get; set; }
        public ICollection<Pedido>? Pedidos { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
	