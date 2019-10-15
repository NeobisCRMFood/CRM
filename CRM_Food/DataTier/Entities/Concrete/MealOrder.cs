using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class MealOrder
    {
        //[JsonIgnore]
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public int MealId { get; set; }
        [JsonIgnore]
        public Meal Meal { get; set; }
        public int Quantity { get; set; }
        private int? mealOrderStatusId;
        [JsonIgnore]
        public int MealOrderStatusId
        {
            get { return mealOrderStatusId ?? 1; }
            set { mealOrderStatusId = value; }
        }
        [JsonIgnore]
        public MealOrderStatus MealOrderStatus { get; set; }
    }
}
