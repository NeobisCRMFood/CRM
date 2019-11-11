using DataTier.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Abstract
{
    public interface IEFDbContextRepository
    {
        IEnumerable<User> Users { get;}
        IEnumerable<Table> Tables {get;}
        IEnumerable<Category> Categories {get;}
        IEnumerable<Meal> Meals {get;}
        IEnumerable<Order> Orders {get;}
        IEnumerable<MealOrder> MealOrders {get;}
    }
}
