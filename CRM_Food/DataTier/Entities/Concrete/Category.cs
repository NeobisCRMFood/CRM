using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
namespace DataTier.Entities.Concrete
{
    public class Category
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        [JsonIgnore]
        public Department Department { get; set; }
        [JsonIgnore]
        public ICollection<Meal> Meals { get; set; }
        public string ImageURL { get; set; }
        public Category()
        {
            Meals = new List<Meal>();
        }
    }
}
