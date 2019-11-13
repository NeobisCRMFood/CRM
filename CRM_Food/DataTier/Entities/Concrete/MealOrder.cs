using DataTier.Entities.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class MealOrder
    {
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public int MealId { get; set; }
        [JsonIgnore]
        public Meal Meal { get; set; }
        public int OrderedQuantity { get; set; }
        public int FinishedQuantity { get; set; }
        [JsonIgnore]
        public MealOrderStatus MealOrderStatus { get; set; }
    }
}
