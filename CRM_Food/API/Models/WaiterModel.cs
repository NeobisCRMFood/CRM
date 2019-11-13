using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class AddMealOrderModel
    {
        public int OrderId { get; set; }
        public List<AddMealsModel> MealOrders { get; set; }
    }

    public class AddMealsModel
    {
        public int MealId { get; set; }
        public int AddQuantity { get; set; }
    }

    public class DeleteMealOrderModel
    {
        public int OrderId { get; set; }
        public List<DeleteMealsModel> MealOrders { get; set; }
    }

    public class DeleteMealsModel
    {
        public int MealId { get; set; }
        public int DeleteQuantity { get; set; }
    }

    public class DeleteFreezedMealModel
    {
        public int OrderId { get; set; }
        public int MealId { get; set; }
    }

    public class ChequeModel
    {
        public int OrderId { get; set; }
    }
}
