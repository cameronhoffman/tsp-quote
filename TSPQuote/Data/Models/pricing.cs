using System;
using System.Collections.Generic;
using System.Text;

namespace TSPQuote.Data.Models
{
    public class print_price
    {
        [Newtonsoft.Json.JsonProperty]
        public float count_10_20 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_21_40 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_41_70 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_71_150 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_151_300 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_301_500 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_501_1000 { get; set; }
        [Newtonsoft.Json.JsonProperty]
        public float count_1001_2000 { get; set; }
    }

    public class priceobject
    {
        public List<print_price> prices { get; set; }
    }
}
