using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Auditoria
{
    public class AUser
    {
        public AUser(User user)
        {
            Name= user.Name;
            Email= user.Email;
            Phone = user.Phone;
            IdRol = user.IdRol;
            Password= user.Password;
            IsActive = user.IsActive;
            IdTienda= user.IdTienda;
        }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? IdRol { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
        public int? IdTienda { get; set; }
    }
}
