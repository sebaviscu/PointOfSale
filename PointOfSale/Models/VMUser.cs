namespace PointOfSale.Models
{
    public class VMUser
    {
        public int IdUsers { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? IdRol { get; set; }
        public string? NameRol { get; set; }
        public string? Password { get; set; }
        public byte[]? Photo { get; set; }
        public string? PhotoBase64 { get; set; }
        public int? IsActive { get; set; }
        public bool? SinHorario { get; set; }
        public DateTime? ModificationDate { get; set; }
		public string? ModificationUser { get; set; }
        public int? IdTienda { get; set; }
		public string? TiendaName { get; set; }

        public ICollection<VMHorario> Horarios { get; set; }
	}
}
