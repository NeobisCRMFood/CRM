using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class CreateOrderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public int TableId { get; set; }
        [JsonIgnore]
        public Table Table { get; set; }
        [JsonIgnore]
        public DateTime DateTimeOrdered { get; set; }
        [JsonIgnore]
        public DateTime? DateTimeClosed { get; set; }
        [JsonIgnore]
        public OrderStatus OrderStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public string Comment { get; set; }
        public ICollection<MealOrder> MealOrders { get; set; }
        public CreateOrderModel()

        {
            MealOrders = new List<MealOrder>();
        }
    }
}
