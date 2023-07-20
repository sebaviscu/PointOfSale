using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PointOfSale.Model
{
	public partial class Tienda
	{
		public int IdTienda { get; set; }
		public string? Nombre { get; set; }
		public DateTime? ModificationDate { get; set; }
		public string? ModificationUser { get; set; }
	}
}
