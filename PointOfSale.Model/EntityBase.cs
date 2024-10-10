using PointOfSale.Model.Configuracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public abstract class EntityBase
    {
        public int Id { get; set; }

        [NonUpdatable]
        public DateTime? RegistrationDate { get; set; }

        [NonUpdatable]
        public string? RegistrationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
