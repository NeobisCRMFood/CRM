using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using DataTier.Entities.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    //[Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly EFDbContext _context;
        public AdminController(EFDbContext context)
        {
            _context = context;
        }
        [Route("BookTable")]
        [HttpPost]
        public async Task<IActionResult> BookTable([FromBody] BookModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var table = _context.Tables.FirstOrDefault(t => t.Id == model.TableId);
            if (table == null)
            {
                return BadRequest();
            }
            if (table.Status != TableStatus.Busy && table.Status != TableStatus.Booked)
            {
                table.Status = TableStatus.Booked;
                table.BookDate = model.BookDate;
            }
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Route("TotalSumDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalSumDateRange([FromBody] DateRange model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
            {
                return BadRequest();
            }
            if (table.Status != TableStatus.Busy && table.Status != TableStatus.Booked)
            {
                table.Status = TableStatus.Free;
                table.BookDate = null;
            }
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
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
                .Where(m => m.Category.DepartmentId == 1)
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
                .Where(m => m.Category.DepartmentId == 2)
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
    }
}