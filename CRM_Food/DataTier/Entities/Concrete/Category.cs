using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<Meal> Meals { get; set; }
        public Category()
        {
            Meals = new List<Meal>();
        }
    }
}
