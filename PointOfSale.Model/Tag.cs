
namespace PointOfSale.Model
{
    public class Tag
    {
        public int IdTag { get; set; }
        public string Nombre { get; set; }
        public string Color { get; set; }

        public virtual List<ProductTag>? ProductTags { get; set; } = new List<ProductTag>();

    }
}
