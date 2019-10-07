using DataTier.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Abstract
{
    public interface IEFDbContextRepository
    {
        IEnumerable<Role> Roles { get; }
        IEnumerable<User> Users { get;}
        IEnumerable<Department> Departments  {get;}
        IEnumerable<Table> Tables {get;}
        IEnumerable<OrderStatus> OrderStatuses {get;}
        IEnumerable<Category> Categories {get;}
        IEnumerable<Meal> Meals {get;}
        IEnumerable<Order> Orders {get;}
        IEnumerable<MealOrderStatus> MealOrderStatuses {get;}
        IEnumerable<MealOrder> MealOrders {get;}
    }
}
