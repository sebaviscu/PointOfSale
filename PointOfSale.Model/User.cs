using System;
using System.Collections.Generic;

namespace PointOfSale.Model
{
    public partial class User
    {
        public User()
        {
            IsSuperAdmin = false;
        }

        public int IdUsers { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? IdRol { get; set; }
        public string? Password { get; set; }
        public byte[]? Photo { get; set; }
        public bool? IsActive { get; set; }
        public bool? SinHorario { get; set; }
        public bool IsSuperAdmin { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public virtual Rol? IdRolNavigation { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public int? IdTienda { get; set; }
        public Tienda? Tienda { get; set; }
        public virtual ICollection<Horario>? Horarios { get; set; }
        public virtual ICollection<HistorialLogin>? HistorialLogins { get; set; }

        public bool IsAdmin
        {
            get { return IdRol == 1; }
        }
    }
}
