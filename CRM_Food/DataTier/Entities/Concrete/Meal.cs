using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Meal
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public Category Category { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Заполните описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Заполните цену")]
        public decimal Price { get; set; }
        public int? Weight { get; set; }
        public string ImageURL { get; set; }

        [JsonIgnore]
        public ICollection<MealOrder> MealOrders { get; set; }
        public Meal()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
