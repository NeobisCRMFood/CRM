using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class OrderStatus
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
