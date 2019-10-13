using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class MealOrderStatus
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<MealOrder> MealOrders { get; set; }
        public MealOrderStatus()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
