using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Department
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Category> Categories { get; set; }
        public Department()
        {
            Categories = new List<Category>();
        }
    }
}
