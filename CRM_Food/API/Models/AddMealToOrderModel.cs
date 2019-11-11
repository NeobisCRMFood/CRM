using DataTier.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class AddMealToOrderModel
    {
        public int OrderId { get; set; }
        public List<MealOrder> Meals { get; set; }
    }
}
