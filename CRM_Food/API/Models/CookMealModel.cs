using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class MealReadyModel
    {
        public int OrderId { get; set; }
        public int MealId { get; set; }
        public int FinishedQuantity { get; set; }
    }

    public class FreezeMealModel : CloseMealModel
    {
        public int FreezedMeals { get; set; }
    }

    public class CloseMealModel
    {
        public int OrderId { get; set; }
        public int MealId { get; set; }
    }
}
