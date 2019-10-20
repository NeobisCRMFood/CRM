using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class MealOrderStatus
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<MealOrder> MealOrders { get; set; }
        public MealOrderStatus()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
