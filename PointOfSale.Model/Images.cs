using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Images
    {
        public Images(string imageBase64, string flag)
        {
            ImageBase64 = imageBase64;
            Flag = $"[[{flag}]]";
        }

        public string ImageBase64 { get; set; }
        public string Flag { get; set; }
    }
}
