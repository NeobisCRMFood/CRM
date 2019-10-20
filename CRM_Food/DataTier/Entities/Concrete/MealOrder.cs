using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class MealOrder
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int MealId { get; set; }
        public Meal Meal { get; set; }
        public int Quantity { get; set; }
        private int? mealOrderStatusId;
        public int MealOrderStatusId
        {
            get { return mealOrderStatusId ?? 1; }
            set { mealOrderStatusId = value; }
        }
        public MealOrderStatus MealOrderStatus { get; set; }
    }
}
