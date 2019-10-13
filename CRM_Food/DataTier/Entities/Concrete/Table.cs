using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Table
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Order> Orders { get; set; }
        public Table()
        {
            Orders = new List<Order>();
        }
    }
}
