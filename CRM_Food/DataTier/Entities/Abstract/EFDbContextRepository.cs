using System;
using System.Collections.Generic;
using System.Text;
using DataTier.Entities.Concrete;

namespace DataTier.Entities.Abstract
{
    public class EFDbContextRepository : IEFDbContextRepository
    {
        private EFDbContext _context = new EFDbContext();

        public IEnumerable<User> Users { get { return _context.Users; } }
        public IEnumerable<Department> Departments { get { return _context.Departments; } }
        public IEnumerable<Table> Tables { get { return _context.Tables; } }
        public IEnumerable<Category> Categories { get { return _context.Categories; } }
        public IEnumerable<Meal> Meals { get { return _context.Meals; } }
        public IEnumerable<Order> Orders { get { return _context.Orders; } }
        public IEnumerable<MealOrder> MealOrders { get { return _context.MealOrders; } }
    }
}
