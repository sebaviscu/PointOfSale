using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Notifications
    {
        public int IdNotifications { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
