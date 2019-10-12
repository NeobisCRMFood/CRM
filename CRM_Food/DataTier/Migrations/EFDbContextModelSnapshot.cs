﻿// <auto-generated />
using System;
using DataTier.Entities.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataTier.Migrations
{
    [DbContext(typeof(EFDbContext))]
    partial class EFDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DataTier.Entities.Concrete.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DepartmentId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Department", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Meal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CategoryId");

                    b.Property<string>("Description");

                    b.Property<string>("ImageURL");

                    b.Property<string>("Name");

                    b.Property<decimal>("Price");

                    b.Property<int?>("Weight");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Meals");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.MealOrder", b =>
                {
                    b.Property<int>("MealId");

                    b.Property<int>("OrderId");

                    b.Property<int>("MealOrderStatusId");

                    b.Property<int>("Quantity");

                    b.HasKey("MealId", "OrderId");

                    b.HasIndex("MealOrderStatusId");

                    b.HasIndex("OrderId");

                    b.ToTable("MealOrders");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.MealOrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("MealOrderStatuses");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comment");

                    b.Property<DateTime?>("DateTimeClosed");

                    b.Property<DateTime>("DateTimeOrdered");

                    b.Property<int>("OrderStatusId");

                    b.Property<int>("TableId");

                    b.Property<decimal>("TotalPrice");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("OrderStatusId");

                    b.HasIndex("TableId");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.OrderStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("OrderStatuses");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Table", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Tables");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comment");

                    b.Property<DateTime>("DateBorn");

                    b.Property<string>("FirstName");

                    b.Property<string>("Gender");

                    b.Property<string>("LastName");

                    b.Property<string>("Login");

                    b.Property<string>("MiddleName");

                    b.Property<string>("Password");

                    b.Property<string>("PhoneNumber");

                    b.Property<int>("RoleId");

                    b.Property<DateTime>("StartWorkDay");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Category", b =>
                {
                    b.HasOne("DataTier.Entities.Concrete.Department", "Department")
                        .WithMany("Categories")
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Meal", b =>
                {
                    b.HasOne("DataTier.Entities.Concrete.Category")
                        .WithMany("Meals")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.MealOrder", b =>
                {
                    b.HasOne("DataTier.Entities.Concrete.Meal", "Meal")
                        .WithMany("MealOrders")
                        .HasForeignKey("MealId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataTier.Entities.Concrete.MealOrderStatus", "MealOrderStatus")
                        .WithMany("MealOrders")
                        .HasForeignKey("MealOrderStatusId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataTier.Entities.Concrete.Order", "Order")
                        .WithMany("MealOrders")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.Order", b =>
                {
                    b.HasOne("DataTier.Entities.Concrete.OrderStatus", "OrderStatus")
                        .WithMany()
                        .HasForeignKey("OrderStatusId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataTier.Entities.Concrete.Table", "Table")
                        .WithMany("Orders")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataTier.Entities.Concrete.User", "User")
                        .WithMany("Orders")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataTier.Entities.Concrete.User", b =>
                {
                    b.HasOne("DataTier.Entities.Concrete.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
