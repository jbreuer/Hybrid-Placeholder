namespace HybridPlaceholderExample.Models
{
    using System;
    using Newtonsoft.Json.Linq;

    public class HybridExample
    {
        public JToken Heading { get; set; }
        public string Date { get; set; }
        
        public JToken Text { get; set; }
    }
}