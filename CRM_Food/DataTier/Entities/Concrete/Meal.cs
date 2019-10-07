using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Meal
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? Weight { get; set; }
        public string ImageURL { get; set; }

        public ICollection<MealOrder> MealOrders { get; set; }
        public Meal()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
