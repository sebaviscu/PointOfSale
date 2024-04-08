using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public partial class Turno
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        public Turno()
        {

        }

        public Turno(int idTienda, string usuario)
        {
            FechaInicio = DateTimeNowArg;
            IdTienda = idTienda;
            RegistrationUser = usuario;
            RegistrationDate= DateTimeNowArg;
        }
        public int IdTurno { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Descripcion { get; set; }
        public int IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationUser { get; set; }
        public string? ModificationUser { get; set; }

		public virtual ICollection<Sale> Sales { get; set; }

	}
}
