using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Order
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public int TableId { get; set; }
        [JsonIgnore]
        public Table Table { get; set; }
        private DateTime? dateTimeOrdered;
        [JsonIgnore]
        public DateTime DateTimeOrdered
        {
            get { return dateTimeOrdered ?? DateTime.UtcNow; }
            set { dateTimeOrdered = value; }
        }
        [JsonIgnore]
        public DateTime? DateTimeClosed { get; set; }
        private int? orderStatusId;
        [JsonIgnore]
        public int OrderStatusId
        {
            get { return orderStatusId ?? 1; }
            set { orderStatusId = value; }
        }
        [JsonIgnore]
        public OrderStatus OrderStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public string Comment { get; set; }
        public ICollection<MealOrder> MealOrders { get; set; }
        public Order()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
