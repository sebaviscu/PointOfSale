using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMUserLogin
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PassWord { get; set; }
        public bool KeepLoggedIn { get; set; }

        public bool IsAdmin { get; set; }

        public int? TiendaId { get; set; }
        public bool FirstLogin { get; set; }
    }
}
