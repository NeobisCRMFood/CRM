using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
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

        [Route("getMeals")]
        [HttpGet]
        public IActionResult GetMeals()
        {
            var meals = _context.Meals.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                description = m.Description,
                deprtmentId = m.Category.Department,
                categoryId = m.CategoryId,
                category = m.Category.Name,
                price = m.Price,
                weight = m.Weight,
                status = m.MealStatus.ToString(),
                image = m.ImageURL
            });
            return Ok(meals);
        }

        [Route("getWaiters")]
        [HttpGet]
        public IActionResult GetWaiters()
        {
            var waiters = _context.Users.Where(u => u.Role == Role.waiter).Select(u => new
            {
                id = u.Id,
                name = u.LastName + " " + u.FirstName + " " + u.MiddleName,
                login = u.Login,
                password = u.Password
            });
            return Ok(waiters);
        }

        [Route("getBooks")]
        [HttpGet]
        public IActionResult GetBooks()
        {
            var books = _context.Books.Select(b => new 
            {
                b.Id,
                b.ClientName,
                b.BookDate,
                b.MenQuantity,
                b.TableId,
                b.PhoneNumber
            });
            return Ok(books);
        }
        #region Статистика
        [Route("waiterStatistics/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatistics([FromRoute] int id)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter" });
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

            var statiscticsToday = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
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

            var statiscticsWeek = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Select(o => o.TotalPrice)
                .Sum()
            });

            var statiscticsMonth = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .Sum()
            });

            return Ok(new { statisctics, statiscticsToday, statiscticsWeek, statiscticsMonth});
        }
        
        [Route("totalSums")]
        [HttpGet]
        public async Task<IActionResult> TotalSum()
        {
            var totalSum = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumToday = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumLastDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-2) && o.DateTimeClosed <= DateTime.Now.AddDays(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumLastWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-14) && o.DateTimeClosed <= DateTime.Now.AddDays(-7))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumLastMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-2) && o.DateTimeClosed <= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalPrices = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice);
            var sum = await totalPrices
                .SumAsync();
            var count = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .CountAsync();
            var totalSumAverage = sum / count;

            return Ok(new {  
                totalSum, 
                totalSumToday,
                totalSumMonth,
                totalSumLastMonth,
                totalSumWeek,
                totalSumLastWeek,
                totalSumDay,
                totalSumLastDay,
                totalSumAverage
            });
        }

        [Route("totalOrders")]
        [HttpGet]
        public async Task<IActionResult> TotalOrders()
        {
            var totalOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .CountAsync();

            var totalOrdersToday = await _context.Orders
               .Where(o => o.OrderStatus == OrderStatus.NotActive)
               .Where(o => o.DateTimeClosed >= DateTime.Today)
               .CountAsync();

            var totalOrdersDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .CountAsync();

            var totalOrdersLastDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-2) && o.DateTimeClosed <= DateTime.Now.AddDays(-1))
                .CountAsync();

            var totalOrdersWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .CountAsync();

            var totalOrdersLastWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-14) && o.DateTimeClosed <= DateTime.Now.AddDays(-7))
                .CountAsync();

            var totalOrdersMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .CountAsync();

            var totalOrdersLastMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-2) && o.DateTimeClosed <= DateTime.Now.AddMonths(-1))
                .CountAsync();

            return Ok(new 
            { 
                totalOrders, 
                totalOrdersToday, 
                totalOrdersMonth,
                totalOrdersLastMonth,
                totalOrdersWeek,
                totalOrdersLastWeek,
                totalOrdersDay,
                totalOrdersLastDay
            });
        }

        [Route("kitchenTotalSums")]
        [HttpGet]
        public async Task<IActionResult> KitchenTotalSums()
        {
            var totalSum = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price)
                .SumAsync();

            var totalSumMonth = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price)
                .SumAsync();

            var totalSumWeek = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price)
                .SumAsync();

            var totalSumDay = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price)
                .SumAsync();

            return Ok(new { 
                totalSum, 
                totalSumMonth, 
                totalSumWeek, 
                totalSumDay
            });
        }

        [Route("kitchenTotalMeals")]
        [HttpGet]
        public async Task<IActionResult> KitchenTotalMeals()
        {
            var totalMeals = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.MealId)
                .CountAsync();

            var totalMealsMonth = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.MealId)
                .CountAsync();

            var totalMealsWeek = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.MealId)
                .CountAsync();

            var totalMealsToday = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.MealId)
                .CountAsync();

            return Ok(new 
            {
                totalMeals,
                totalMealsMonth,
                totalMealsWeek,
                totalMealsToday
            });
        }

        [Route("barTotalMeals")]
        [HttpGet]
        public async Task<IActionResult> BarTotalMeals()
        {
            var totalMeals = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.MealId)
                .CountAsync();

            var totalMealsMonth = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.MealId)
                .CountAsync();

            var totalMealsWeek = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.MealId)
                .CountAsync();

            var totalMealsToday = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.MealId)
                .CountAsync();

            return Ok(new
            {
                totalMeals,
                totalMealsMonth,
                totalMealsWeek,
                totalMealsToday
            });
        }

        [Route("barTotalSums")]
        [HttpGet]
        public IActionResult BarTotalSums()
        {
            var totalSum = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price)
                .Sum();

            var totalSumMonth = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price)
                .Sum();

            var totalSumWeek = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price)
                .Sum();

            var totalSumDay = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price)
                .Sum();

            return Ok(new { totalSum, totalSumMonth, totalSumWeek, totalSumDay });
        }

        [Route("topMeals")]
        [HttpGet]
        public IActionResult SalesByMeal()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        #region Топ блюд по сезонам

        [Route("topMealsWinter")]
        [HttpGet]
        public IActionResult SalesByMealWinter()
        {
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                var leapYearMeals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 29))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(leapYearMeals);
            }

            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 28))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topMealsSpring")]
        [HttpGet]
        public IActionResult SalesByMealSpring()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 3, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 5, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("topMealsSummer")]
        [HttpGet]
        public IActionResult SalesByMealSummer()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 6, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 9, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topMealsAutumn")]
        [HttpGet]
        public IActionResult SalesByMealAutumn()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 10, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 11, 30))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        #endregion

        [Route("topDrinks")]
        [HttpGet]
        public IActionResult SalesByDrink()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        #region Топ алкоголя по сезонам

        [Route("topDrinksWinter")]
        [HttpGet]
        public IActionResult SalesByMealDrinks()
        {
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                var leapYearMeals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 29))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
                return Ok(leapYearMeals);
            }

            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 28))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topDrinksSpring")]
        [HttpGet]
        public IActionResult SalesByDrinksSpring()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 3, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 5, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("topDrinksSummer")]
        [HttpGet]
        public IActionResult SalesByDrinksSummer()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 6, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 9, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topDrinksAutumn")]
        [HttpGet]
        public IActionResult SalesByDrinksAutumn()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 10, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 11, 30))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        #endregion

        [Route("transactionHistory")]
        [HttpGet]
        public IActionResult LatestOrders()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.DateTimeOrdered)
                .Select(o => new
                {
                    orderId = o.Id,
                    waiterName = o.User.FirstName + " " + o.User.LastName,
                    orderDate = o.DateTimeOrdered,
                    mealsCount = o.MealOrders.Count(),
                    status = o.OrderStatus.ToString(),
                    totalPrice = o.TotalPrice
                });
            return Ok(orders);
        }

        [Route("bestWaiterLastMonth")]
        [HttpGet]
        public IActionResult BestWaiter()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            if (users == null)
            {
                return BadRequest(new { status = "error", message = "Have no waiters" });
            }
            DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
            DateTime finish = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1));
            var top = users.Select(u => new
            {
                id = u.Id,
                name = u.LastName + " " +u.FirstName,
                orderCount = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= start && o.DateTimeClosed <= finish)
                .Count()
            })
            .OrderByDescending(u => u.orderCount).Take(1);
            
            return Ok(top);
        }

        [Route("waiterOrderTop")]
        [HttpGet]
        public IActionResult WaiterTop()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            if (users == null)
            {
                return BadRequest(new { status = "error", message = "Have no waiters"});
            }
            var top = users.Select(u => new
            {
                id = u.Id,
                name = u.LastName + " " + u.FirstName,
                orderCount = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Count()
            })
            .OrderByDescending(u => u.orderCount);

            return Ok(top);
        }

        [Route("waiterSumTop")]
        [HttpGet]
        public IActionResult WaiterSumTop()
        {
            var waitersSumTop = _context.Users
                .Where(u => u.Role == Role.waiter)
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.LastName + " " + u.FirstName,
                    sum = _context.MealOrders.Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => mo.Order.TotalPrice)
                    .Sum()
                }).OrderBy(mo => mo.sum);
            return Ok(waitersSumTop);
        }

        [Route("kitchenOrdersStatistics")]
        [HttpGet]
        public IActionResult KitchenOrdersStatistics()
        {
            var ordersTop = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(ordersTop);
        }

        [Route("kitchenSumStatistics")]
        [HttpGet]
        public IActionResult KitchenSumStatistics()
        {
            var sumTop = _context.Meals
               .Where(m => m.Category.Department == Department.Kitchen)
               .Select(m => new
               {
                   id = m.Id,
                   name = m.Name,
                   sum = m.Price * m.MealOrders
                   .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                   .Select(mo => new
                   {
                       mo.OrderId
                   }).Count()
               })
               .OrderByDescending(mo => mo.sum);
            return Ok(sumTop);
        }

        [Route("kitchenWaiterMealStatistics")]
        [HttpGet]
        public IActionResult KitchenWaiterMealStatistics()
        {
            var waitersMealTop = _context.Users
                .Where(u => u.Role == Role.waiter)
                .Select(u => new
                {
                    userId = u.Id,
                    userName = u.LastName + " " + u.FirstName,
                    meals = _context.Meals.Where(m => m.Category.Department == Department.Kitchen)
                    .Select(m => new
                    {
                        mealId = m.Id,
                        mealName = m.Name,
                        count = m.MealOrders
                        .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                        .Where(mo => mo.Order.UserId == u.Id)
                        .Select(mo => new
                        {
                            mo.OrderId
                        })
                    .Count()
                }).OrderByDescending(mo => mo.count)
            });
            return Ok(waitersMealTop);
        }

        [Route("kitchenWaiterSumStatistics")]
        [HttpGet]
        public IActionResult KitchenWaiterSumStatistics()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var waitersSumTop = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.Meals.Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    mealId  = m.Id,
                    mealName = m.Name,
                    sum = m.Price * m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => new
                    {
                        mo.OrderId
                    }).Count()
                }).OrderByDescending(mo => mo.sum)
            });
            return Ok(waitersSumTop);
        }

        [Route("barOrdersStatistics")]
        [HttpGet]
        public IActionResult BarOrdersStatistics()
        {
            var ordersTop = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(ordersTop);
        }

        [Route("barSumStatistics")]
        [HttpGet]
        public IActionResult BarSumStatistics()
        {
            var sumTop = _context.Meals
                   .Where(m => m.Category.Department == Department.Bar)
                   .Select(m => new
                   {
                       id = m.Id,
                       name = m.Name,
                       sum = m.Price * m.MealOrders
                       .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                       .Select(mo => new
                       {
                           mo.OrderId
                       }).Count()
                   })
                   .OrderByDescending(mo => mo.sum);
            return Ok(sumTop);
        }

        [Route("barWaiterMealStatistics")]
        [HttpGet]
        public IActionResult BarWaiterMealStatistics()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var waiterMealTop = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    mealId = m.Id,
                    mealName = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => new
                    {
                        mo.OrderId
                    }).Count()
                }).OrderByDescending(mo => mo.count)
            });
            return Ok(waiterMealTop);
        }

        [Route("barWaiterSumStatistics")]
        [HttpGet]
        public IActionResult BarWaiterSumStatistics()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var waitersSumTop = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    mealId = m.Id,
                    mealName = m.Name,
                    sum = m.Price * m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => new
                    {
                        mo.OrderId
                    }).Count()
                }).OrderByDescending(mo => mo.sum)
            });
            return Ok(waitersSumTop);
        }

        #endregion

        [Route("bookTable")]
        [HttpPost]
        public async Task<IActionResult> BookTable([FromBody] BookModel model)
        {
            //Проверка на модель, так как автоматически присваиваются данные для модели
            if (BookIsNull(model))
            {
                return BadRequest(new { status = "error", message = "Invalid Json model"});
            }
            var table = _context.Tables.FirstOrDefault(t => t.Id == model.TableId);
            if (table == null)
            {
                return NotFound(new { status = "error", message = "Table was not found"});
            }
            if (table.Status == TableStatus.Busy && table.Status == TableStatus.Booked)
            {
                return BadRequest(new { status = "error", message = "Table is busy or booked yet" });
            }
            var book = new Book()
            {
                TableId = model.TableId,
                ClientName = model.ClientName,
                BookDate = model.BookDate,
                MenQuantity = model.MenQuantity,
                PhoneNumber = model.PhoneNumber
            };
            _context.Books.Add(book);
            table.Status = TableStatus.Booked;
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Table is booked" });
        }

        [Route("waiterStatisticsRange/{id}")]
        [HttpPost]
        public async Task<IActionResult> GetWaiterStatisticsRange([FromRoute] int id, [FromBody] DateRange model)
        {
            if (DateIsNull(model))
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

        [Route("totalSumDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalSumDateRange([FromBody] DateRange model)
        {
            if (DateIsNull(model))
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
        [Route("totalOrdersDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalOrdersDateRange([FromBody] DateRange model)
        {
            if (DateIsNull(model))
            {
                return BadRequest(new { status = "error", message = "Date model is not valid" });
            }
            var totalPrices = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .CountAsync();
            return Ok(totalPrices);
        }

        [HttpDelete("deleteBook/{id}")]
        public async Task<IActionResult> DeleteBook([FromRoute] int id)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
            {
                return NotFound(new { status = "error", message = "Table was not found" });
            }
            if (table.Status != TableStatus.Booked)
            {
                return BadRequest(new { status = "error", message = "Table is free or busy" });
            }
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            _context.Books.Remove(book);
            table.Status = TableStatus.Free;
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Book is deleted" });
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
                    return Ok(meal);
                }
                else if (meal.MealStatus == MealStatus.HaveNot)
                {
                    meal.MealStatus = MealStatus.Have;
                    await _context.SaveChangesAsync();
                    return Ok(meal);
                }
            }
            return NotFound(new { status = "error", message = "Meal or drink was not Found"});
        }

        [Route("deleteMealsOrder")]
        [HttpPost]
        public async Task<IActionResult> DeleteMealFromOrder([FromBody] DeleteMealOrderModel model)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null)
            {
                return NotFound(new { status = "error", message = "Order was not found" });
            }
            foreach (var item in model.MealOrders)
            {
                var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == item.MealId);
                if (meal == null)
                {
                    return NotFound(new { status = "error", message = "Meal was not found in database" });
                }

                var mealOrder = _context.MealOrders.FirstOrDefault(mo => mo.OrderId == order.Id && mo.MealId == item.MealId);

                if (mealOrder == null)
                {
                    return NotFound(new { status = "error", message = $"Meal {item.MealId} in Order {order.Id} is not exist" });
                }
                if (mealOrder.MealOrderStatus == MealOrderStatus.Ready)
                {
                    if (mealOrder.OrderedQuantity == item.DeleteQuantity)
                    {
                        _context.MealOrders.Remove(mealOrder);
                    }
                    else if (mealOrder.OrderedQuantity > item.DeleteQuantity)
                    {
                        mealOrder.OrderedQuantity -= item.DeleteQuantity;
                    }
                    else if (mealOrder.OrderedQuantity < item.DeleteQuantity)
                    {
                        return BadRequest(new { status = "error", message = "Delete quantity can't be more than ordered quantity" });
                    }
                    if (mealOrder.OrderedQuantity <= mealOrder.FinishedQuantity)
                    {
                        mealOrder.MealOrderStatus = MealOrderStatus.Ready;
                    }
                    else
                    {
                        mealOrder.MealOrderStatus = MealOrderStatus.NotReady;
                    }
                }
                else
                {
                    return BadRequest(new { status = "error", message = "This method can't delete meal from not ready or freezed MealOrder" });
                }
            }
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Meals was deleted from order" });
        }

        private bool DateIsNull(DateRange dateRange)
        {
            if (dateRange.StartDate <= DateTime.Parse("01.01.0001 0:00:00") || dateRange.EndDate <= DateTime.Parse("01.01.0001 0:00:00"))
            {
                return true;
            }
            return false;
        }
        private bool BookIsNull(BookModel model)
        {
            if (model.BookDate <= DateTime.Parse("01.01.0001 0:00:00") && model.TableId <= 0)
            {
                return true;
            }
            return false;
        }
    }
}