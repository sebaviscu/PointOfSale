namespace PointOfSale.Web.Models.Paginado
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public SearchCriteria Search { get; set; }
        public OrderCriteria[] Order { get; set; }
        public ColumnCriteria[] Columns { get; set; }
    }

    public class SearchCriteria
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class OrderCriteria
    {
        public int Column { get; set; }
        public string Dir { get; set; } // "asc" o "desc"
    }

    public class ColumnCriteria
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public SearchCriteria Search { get; set; }
    }

}
