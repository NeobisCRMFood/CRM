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
        private DateTime? dateTimeOrdered;
        public DateTime DateTimeOrdered
        {
            get { return dateTimeOrdered ?? DateTime.Now; }
            set { dateTimeOrdered = value; }
        }
        public DateTime? DateTimeClosed { get; set; }

        private int? orderStatusId;
        public int OrderStatusId
        {
            get { return orderStatusId ?? 1; }
            set { orderStatusId = value; }
        }
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
