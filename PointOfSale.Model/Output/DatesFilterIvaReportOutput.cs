using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model.Output
{
    public class DatesFilterIvaReportOutput
    {
        public DatesFilterIvaReportOutput(string id, string text)
        {
            DateId = id;
            DateText = text;
        }
        public string? DateId { get; set; }
        public string? DateText { get; set; }
    }
}
