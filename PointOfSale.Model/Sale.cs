using System;
using System.Collections.Generic;

namespace PointOfSale.Model
{
    public partial class Sale
    {
        public Sale()
        {
            DetailSales = new HashSet<DetailSale>();
        }

        public int IdSale { get; set; }
        public string? SaleNumber { get; set; }
        public int? IdTypeDocumentSale { get; set; }
        public int? IdUsers { get; set; }
        public string? CustomerDocument { get; set; }
        public string? ClientName { get; set; }
        public decimal? Total { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public virtual TypeDocumentSale? TypeDocumentSaleNavigation { get; set; }
        public virtual User? IdUsersNavigation { get; set; }
        public virtual ICollection<DetailSale> DetailSales { get; set; }
        public int IdTurno { get; set; }
        public Turno Turno { get; set; }
        public int? IdClienteMovimiento { get; set; }
        public ClienteMovimiento? ClienteMovimiento { get; set; }
        public int IdTienda { get; set; }
    }
}
