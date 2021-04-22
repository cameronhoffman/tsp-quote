using System;
using System.Collections.Generic;
using System.Text;

namespace TSPQuote.Data.Models
{
    public class product
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public float Margin { get; set; }
        public float SetupFee { get; set; }
        public List<print> Prints { get; set; }
        public List<quantity> Quantities { get; set; }

    }
    public class print
    {
        public string location { get; set; }
        public int colors { get; set; }
        public bool baseprint { get; set; }
    }
    public class quantity
    {
        public string range { get; set; }
        public float totalprice { get; set; }
    }
}
