﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Gastos
    {
        public int IdGastos { get; set; }
        public int IdTipoGasto { get; set; }
        public TipoDeGasto? TipoDeGasto { get; set; }
        public decimal Importe {get; set; }
        public int? IdUsuario { get; set; }
        public User? User { get; set; }
        public string? Comentario { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public int IdTienda { get; set; }

    }
}