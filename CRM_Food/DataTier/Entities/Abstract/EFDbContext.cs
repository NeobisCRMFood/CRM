﻿using DataTier.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Abstract
{
    public class EFDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MealOrderStatus> MealOrderStatuses { get; set; }
        public DbSet<MealOrder> MealOrders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql
                (
                "Host=satao.db.elephantsql.com;" +
                "Port=5432;" +
                "Database=bomosrkc;" +
                "Username=bomosrkc;" +
                "Password=DtoD5_DP3-_0qtyQ8rnY-jll5Z3Tel2K;"
                );
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