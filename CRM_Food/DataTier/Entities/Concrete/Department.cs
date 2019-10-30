using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Department
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required(ErrorMessage = "Заполините название")]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Category> Categories { get; set; }
        public Department()
        {
            Categories = new List<Category>();
        }
    }
}
