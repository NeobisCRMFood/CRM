﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    //[Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<FoodHub> _hubContext;
        public AdminController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getMenu")]
        [HttpGet]
        public IActionResult Get_menu()
        {
            var menu = _context.Categories
                .Select(c => new
                {
                    category = c.Name,
                    departmentId = c.Department,
                    departmentName = c.Department.ToString(),
                    meals = c.Meals.Select(m => new
                    {
                        mealId = m.Id,
                        mealName = m.Name,
                        mealWeight = m.Weight,
                        mealStatus = m.MealStatus,
                        price = m.Price
                    })
                });
            return Ok(menu);
        }

        [Route("BookTable")]
        [HttpPost]
        public async Task<IActionResult> BookTable([FromBody] BookModel model)
        {
            //Проверка на модель, так как автоматически присваиваются данные для модели
            if (model.BookDate <= DateTime.Parse("01.01.0001 0:00:00") && model.TableId <= 0)
            {
                return BadRequest(new { status = "error", message = "Invalid Json model"});
            }
            var table = _context.Tables.FirstOrDefault(t => t.Id == model.TableId);
            if (table == null)
            {
                return NotFound(new { status = "error", message = "Table was not found"});
            }
            if (table.Status != TableStatus.Busy && table.Status != TableStatus.Booked)
            {
                table.Status = TableStatus.Booked;
                table.BookDate = model.BookDate;
            }
            else
            {
                return BadRequest(new { status = "error", message = "Table is busy or booked"});
            }
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Table is booked" });
        }

        [Route("TotalSumDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalSumDateRange([FromBody] DateRange model)
        {
            if (model.StartDate <= DateTime.Parse("01.01.0001 0:00:00") || model.EndDate <= DateTime.Parse("01.01.0001 0:00:00"))
            {
                return BadRequest(new { status = "error", message = "Date model is not valid"});
            }
            var totalPrices =  _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }

        [Route("TotalOrdersDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalOrdersDateRange([FromBody] DateRange model)
        {
            if (model.StartDate <= DateTime.Parse("01.01.0001 0:00:00") || model.EndDate <= DateTime.Parse("01.01.0001 0:00:00"))
            {
                return BadRequest(new { status = "error", message = "Date model is not valid" });
            }
            var totalPrices = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .CountAsync();
            return Ok(totalPrices);
        }

        [HttpPut("DeleteBook/{id}")]
        public async Task<IActionResult> DeleteBook([FromRoute] int id)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
            {
                return NotFound(new { status = "error", message = "Table was not found" });
            }
            if (table.Status == TableStatus.Booked)
            {
                table.Status = TableStatus.Free;
                table.BookDate = null;
            }
            else
            {
                return BadRequest(new { status = "error", message = "Table is free or busy"});
            }
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Book is deleted" });
        }

        [Route("getMeals")]
        [HttpGet]
        public IActionResult GetMeals()
        {
            var meals = _context.Meals.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                description = m.Description,
                category = m.Category.Name,
                price = m.Price,
                weight = m.Weight,
                status = m.MealStatus.ToString(),
                image = m.ImageURL
            });
            return Ok(meals);
        }

        [Route("changeMealStatus/{id}")]
        [HttpPut]
        public async Task<IActionResult> ChangeMealStatus([FromRoute] int id)
        {
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == id);
            if (meal != null)
            {
                if (meal.MealStatus == MealStatus.Have)
                {
                    meal.MealStatus = MealStatus.HaveNot;
                    await _context.SaveChangesAsync();

                    //string message = $"Ингредиентов для блюда {meal.Name} не осталось в наличии";
                    //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(GetUserId()));
                    //if (user.Role == Role.admin || user.Role == Role.cook)
                    //{
                    //    await _hubContext.Clients.User(user.Id.ToString()).SendAsync($"Notify", message);
                    //}
                    return Ok(meal);
                }
                else if (meal.MealStatus == MealStatus.HaveNot)
                {
                    meal.MealStatus = MealStatus.Have;
                    await _context.SaveChangesAsync();

                    //string message = $"Ингредиенты для блюда {meal.Name} появились в наличии";
                    //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(GetUserId()));
                    //if (user.Role == Role.admin || user.Role == Role.cook)
                    //{
                    //    await _hubContext.Clients.User(user.Id.ToString()).SendAsync($"Notify", message);
                    //}
                    return Ok(meal);
                }
            }
            return NotFound(new { status = "error", message = "Meal or drink was not Found"});
        }

        [Route("getWaiters")]
        [HttpGet]
        public IActionResult GetWaiters()
        {
            var waiters = _context.Users.Where(u => u.Role == Role.waiter).Select(u => new
            {
                name = u.LastName + " " + u.FirstName + " " + u.MiddleName,
                login = u.Login,
                password = u.Password
            });
            return Ok(waiters);
        }

        #region Статистика
        [Route("getWaiterStatistics/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatistics([FromRoute] int id)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter"});
            }
            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("getWaiterStatisticsToday/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatisticsToday([FromRoute] int id)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter == null)
            {
                return NotFound(new { status = "error", message = "Waiter was not found" });
            }

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter" });
            }
            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("getWaiterStatisticsWeek/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatisticsWeek([FromRoute] int id)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter == null)
            {
                return NotFound(new { status = "error", message = "Waiter was not found"});
            }

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter" });
            }
            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-7))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-7))
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("getWaiterStatisticsMonth/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatisticsMonth([FromRoute] int id)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter == null)
            {
                return NotFound(new { status = "error", message = "Waiter was not found" });
            }

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter" });
            }

            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddMonths(-1))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("getWaiterStatisticsRange/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatisticsRange([FromRoute] int id, [FromBody] DateRange model)
        {
            if (model.StartDate <= DateTime.Parse("01.01.0001 0:00:00") || model.EndDate <= DateTime.Parse("01.01.0001 0:00:00"))
            {
                return BadRequest(new { status = "error", message = "Date model is not valid" });
            }

            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter == null)
            {
                return NotFound(new { status = "error", message = "Waiter was not found" });
            }

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter" });
            }

            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeClosed <= o.DateTimeClosed)
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("TotalSum")]
        [HttpGet]
        public async Task<IActionResult> TotalSum()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }

        [Route("TotalSumMonth")]
        [HttpGet]
        public async Task<IActionResult> TotalSumMonth()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddMonths(-1)).Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }

        [Route("TotalSumWeek")]
        [HttpGet]
        public async Task<IActionResult> TotalSumWeek()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-7)).Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }

        [Route("TotalSumToday")]
        [HttpGet]
        public async Task<IActionResult> TotalSumToday()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Where(o => o.DateTimeClosed >= DateTime.Today).Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }

        [Route("TotalSumDay")]
        [HttpGet]
        public async Task<IActionResult> TotalSumDay()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-1)).Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }

        [Route("TotalSumAverage")]
        [HttpGet]
        public async Task<IActionResult> TotalSumAverage()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            var count = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).CountAsync();
            var totalSum = sum / count;
            return Ok(totalSum);
        }
        [Route("TotalOrders")]
        [HttpGet]
        public async Task<IActionResult> TotalOrders()
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .CountAsync();
            return Ok(orders);
        }

        [Route("TotalOrdersMonth")]
        [HttpGet]
        public async Task<IActionResult> TotalOrdersForMonth()
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddMonths(-1))
                .CountAsync();
            return Ok(orders);
        }

        [Route("TotalOrdersWeek")]
        [HttpGet]
        public async Task<IActionResult> TotalOrdersWeek()
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();
            return Ok(orders);
        }

        [Route("TotalOrdersToday")]
        [HttpGet]
        public async Task<IActionResult> TotalOrdersToday()
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-1))
                .CountAsync();
            return Ok(orders);
        }

        [Route("TotalOrdersDay")]
        [HttpGet]
        public async Task<IActionResult> TotalOrdersDay()
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .CountAsync();
            return Ok(orders);
        }

        [Route("TopMeals")]
        [HttpGet]
        public IActionResult SalesByMeal()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Include(m => m.MealOrders)
                .Select(m => new
                {
                name = m.Name,
                count = m.MealOrders.Select(mo => new
                {
                    count = mo.OrderId
                })
                .Count()
            })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("TopDrinks")]
        [HttpGet]
        public IActionResult SalesByDrink()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Include(m => m.MealOrders)
                .Select(m => new
                {
                name = m.Name,
                count = m.MealOrders
                .Select(mo => new
                {
                    count = mo.OrderId
                })
                .Count()
            })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("LatestOrders")]
        [HttpGet]
        public IActionResult LatestOrders()
        {
            var orders = _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).OrderByDescending(o => o.DateTimeOrdered).Select(o => new
            {
                orderId = o.Id,
                time = o.DateTimeClosed,
                meals = o.MealOrders.Count(),
                totalPrice = o.TotalPrice
            });
            return Ok(orders);
        }

        [Route("WaiterTop")]
        [HttpGet]
        public IActionResult WaiterTop()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            if (users == null)
            {
                return BadRequest();
            }
            var top = users.Select(u => new
            {
                name = u.LastName + u.FirstName,
                orderCount = u.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive).Count()
            }).OrderByDescending(u => u.orderCount);

            return Ok(top);
        }
        #endregion
        private string GetUserId()
        {
            var userId = User.Claims.First(i => i.Type == "UserId").Value;
            return userId;
        }
    }
}