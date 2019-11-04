using System;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    //[Authorize(Roles = "waiter")]
    [Route("api/[controller]")]
    [ApiController]
    public class WaiterController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        public WaiterController(EFDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getOrders")]
        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.Orders
                //.Where(o => o.UserId == GetUserId())
                .Where(o => o.OrderStatus == OrderStatus.Active)
                .Select(o => new
                {
                    id = o.Id,
                    tableId = o.TableId,
                    tableName = o.Table.Name,
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        mealId = mo.MealId,
                        quantity = mo.Quantity
                    })
                });
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
               return NotFound();
            }
            return Ok(order);
        }

        [Route("getTables")]
        [HttpGet]
        public IActionResult GetTables()
        {
            var tables = _context.Tables
                .Select(t => new { id = t.Id, name = t.Name });
            return Ok(tables);
        }

        [Route("getFreeTables")]
        [HttpGet]
        public IActionResult GetFreeTables()
        {
            var tables = _context.Tables
                .Where(t => t.Status == TableStatus.Free)
                .Select(t => new { id = t.Id, name = t.Name});
            return Ok(tables);
        }

        [Route("getMenu")]
        [HttpGet]
        public IActionResult Get_menu()
        {
            var menu = _context.Departments
                .Include(c => c.Categories)
                .Select(d => new
                {
                    departmentId = d.Id,
                    departmentName = d.Name,
                    categories = d.Categories.Select(c => new
                    {
                        categoryId = c.Id,
                        categoryName = c.Name,
                        meals = c.Meals.Select(m => new
                        {
                            mealId = m.Id,
                            mealName = m.Name,
                            mealWeight = m.Weight,
                            mealStatus = m.MealStatus,
                            price = m.Price
                        })
                    })
                });
            return Ok(menu);
        }

        [Route("GetWaiterStatistics")]
        [HttpGet]
        public IActionResult GetWaiterStatistics()
        {
            var statisctics = _context.Users.Where(u => u.Id == GetUserId()).Select(u => new
            {
                orderCount = u.Orders.Count(),

                totalSum = u.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("GetWaiterStatisticsToday")]
        [HttpGet]
        public IActionResult GetWaiterStatisticsToday()
        {
            var statisctics = _context.Users.Where(u => u.Id == GetUserId()).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Select(o => o.TotalPrice).Sum()
            });
            return Ok(statisctics);
        }

        [Route("GetWaiterStatisticsWeek")]
        [HttpGet]
        public IActionResult GetWaiterStatisticsWeek()
        {
            var statisctics = _context.Users.Where(u => u.Id == GetUserId()).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-7))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddDays(-7))
                .Select(o => o.TotalPrice).Sum()
            });
            return Ok(statisctics);
        }

        [Route("GetWaiterStatisticsMonth")]
        [HttpGet]
        public IActionResult GetWaiterStatisticsMonth()
        {
            var statisctics = _context.Users.Where(u => u.Id == GetUserId()).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddMonths(-1))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.UtcNow.AddMonths(-1))
                .Select(o => o.TotalPrice).Sum()
            });
            return Ok(statisctics);
        }

        [Route("GetWaiterStatisticsRange")]
        [HttpPost]
        public IActionResult GetWaiterStatisticsRange([FromBody] DateRange model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var statisctics = _context.Users.Where(u => u.Id == GetUserId()).Select(u => new
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

        [Route("createOrder")]
        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            };

            using (var transaction = _context.Database.BeginTransaction())
            {
                //var userId = User.Claims.First(i => i.Type == "UserId").Value;
                //order.UserId = int.Parse(userId);
                var table = _context.Tables.FirstOrDefault(t => t.Id == order.TableId);
                if (table.Status == TableStatus.Free)
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    var ord = await _context.Orders.Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == order.Id);
                    ord.Table.Status = TableStatus.Busy;
                    await _context.SaveChangesAsync();
                    //if (int.Parse(userId) == order.UserId)
                    //{
                    //    await _hubContext.Clients.User(userId).SendAsync($"Notify", "Поступил заказ");
                    //}
                }
                else
                {
                    return BadRequest();
                }
                transaction.Commit();
            }
            return Ok();
        }

        [Route("addMealToOrder")]
        [HttpPost]
        public async Task<IActionResult> AddMealToOrder([FromBody] MealOrder mealOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == mealOrder.OrderId);
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == mealOrder.MealId);
            if (order != null && meal != null)
            {
                order.DateTimeOrdered = DateTime.UtcNow;
                order.OrderStatus = OrderStatus.Active;
                _context.Entry(order).State = EntityState.Modified;
                _context.MealOrders.Add(mealOrder);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        [Route("closeCheque")]
        [HttpPost]
        public async Task<IActionResult> CloseCheque([FromBody] ChequeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null)
            {
                return BadRequest();
            }
            order.OrderStatus = OrderStatus.NotActive;
            order.TotalPrice = model.TotalPrice;
            order.DateTimeClosed = DateTime.UtcNow;
            order.Table.Status = TableStatus.Free;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return CreatedAtAction("getOrder", new { id = order.Id }, order);
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        private int GetUserId()
        {
            return int.Parse(User.Claims.First(i => i.Type == "UserId").Value);
        }
    }
}