using System.ComponentModel.DataAnnotations;
using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMUserLogin
    {
        public string? Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(3, ErrorMessage = "La contraseña debe tener al menos 3 caracteres.")]
        public string PassWord { get; set; }
        public bool KeepLoggedIn { get; set; }

        public bool IsAdmin { get; set; }

        public int? TiendaId { get; set; }
    }
}
