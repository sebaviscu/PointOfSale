﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public partial class Proveedor
    {
        public int IdProveedor { get; set; }
        public string? Nombre { get; set; }
        public string? NombreContacto { get; set; }
        public string? Cuil { get; set; }
        public string? Telefono { get; set; }
        public string? Telefono2 { get; set; }
        public string? Telefono3 { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? Comentario { get; set; }
        public decimal? Iva { get; set; }
        public TipoFactura? TipoFactura { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public ICollection<Product>? Products { get; set; }
        public ICollection<ProveedorMovimiento>? ProveedorMovimiento { get; set; }
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
