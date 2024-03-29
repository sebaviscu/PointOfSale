﻿using System;
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
        public decimal? MontoEnvioGratis { get; set; }
        public decimal? AumentoWeb { get; set; }
        public string? Whatsapp { get; set; }
        public string? Lunes { get; set; }
        public string? Martes { get; set; }
        public string? Miercoles { get; set; }
        public string? Jueves { get; set; }
        public string? Viernes { get; set; }
        public string? Sabado { get; set; }
        public string? Domingo { get; set; }
        public string? Feriado { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Tiktok { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public bool? Principal { get; set; }
        public IEnumerable<Turno>? Turnos { get; set; }
        public IEnumerable<User>? Usuarios { get; set; }
        public IEnumerable<Vencimiento>? Vencimientos { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
	