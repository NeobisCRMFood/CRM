using DataTier.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Abstract
{
    public class EFDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MealOrder> MealOrders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FoodCRM;Trusted_Connection=True;");

            //optionsBuilder.UseNpgsql
            //    (
            //    "Host=satao.db.elephantsql.com;" +
            //    "Port=5432;" +
            //    "Database=bomosrkc;" +
            //    "Username=bomosrkc;" +
            //    "Password=g-MJuj8CrUKFgszpGzYGAs4AGXy-uGrP;"
            //    );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MealOrder>()
                .HasKey(t => new { t.MealId, t.OrderId });

            modelBuilder.Entity<MealOrder>()
                .HasOne(mo => mo.Meal)
                .WithMany(o => o.MealOrders)
                .HasForeignKey(mo => mo.MealId);

            modelBuilder.Entity<MealOrder>()
                .HasOne(mo => mo.Order)
                .WithMany(o => o.MealOrders)
                .HasForeignKey(mo => mo.OrderId);
        }
    }
}
