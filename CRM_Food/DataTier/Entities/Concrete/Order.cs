using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; }
        public DateTime DateTimeOrdered { get; set; }
        public DateTime? DateTimeClosed { get; set; }
        public int OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public decimal TotalPrice { get; set; }

        public ICollection<MealOrder> MealOrders { get; set; }
        public Order()
        {
            MealOrders = new List<MealOrder>();
        }
    }
}
