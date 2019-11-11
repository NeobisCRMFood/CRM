using System;
using System.Collections.Generic;
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
        private readonly IHubContext<FoodHub> _hubContext;
        public WaiterController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getActiveOrders")]
        [HttpGet]
        public IActionResult GetOrders()
        {
            var orders = _context.Orders
                //.Where(o => o.UserId == GetUserId())
                .Where(o => o.OrderStatus == OrderStatus.Active || o.OrderStatus == OrderStatus.MealCooked || o.OrderStatus == OrderStatus.BarCooked)
                .Select(o => new
                {
                    id = o.Id,
                    tableId = o.TableId,
                    tableName = o.Table.Name,
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        mealId = mo.MealId,
                        status = mo.MealOrderStatus.ToString()
                    })
                });
            return Ok(orders);
        }

        [Route("getFinishedOrders")]
        [HttpGet]
        public IActionResult GetFinishedOrders()
        {
            var orders = _context.Orders
                //.Where(o => o.UserId == GetUserId())
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => new
                {
                    id = o.Id,
                    tableId = o.TableId,
                    tableName = o.Table.Name,
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        mealId = mo.MealId
                    })
                });
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
               return NotFound(new { status = "error", message = "Order was not found"});
            }
            return Ok(order);
        }

        [Route("getTables")]
        [HttpGet]
        public IActionResult GetTables()
        {
            var tables = _context.Tables
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    status = t.Status.ToString()
                });
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
            if (model.StartDate <= DateTime.Parse("01.01.0001 0:00:00") || model.EndDate <= DateTime.Parse("01.01.0001 0:00:00"))
            {
                return BadRequest(new { status = "error", message = "Date model is not valid" });
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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel model)
        {
            Order order = new Order()
            {
                //UserId = int.Parse(User.Claims.FirstOrDefault(i => i.Type == "UserId").Value),
                //User = model.User,
                UserId = model.UserId,
                User = model.User,
                TableId = model.TableId,
                Table = model.Table,
                DateTimeOrdered = DateTime.UtcNow,
                OrderStatus = OrderStatus.Active,
                MealOrders = model.MealOrders,
                Comment = model.Comment
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Order was created"});
        }

        //[Route("createOrder")]
        //[HttpPost]
        //public async Task<IActionResult> CreateOrder([FromBody] Order order)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    };

        //    using (var transaction = _context.Database.BeginTransaction())
        //    {
        //        order.UserId = int.Parse(User.Claims.First(i => i.Type == "UserId").Value);
        //        var table = _context.Tables.FirstOrDefault(t => t.Id == order.TableId);
        //        if (table.Status == TableStatus.Free)
        //        {
        //            _context.Orders.Add(order);
        //            await _context.SaveChangesAsync();
        //            var ord = await _context.Orders.Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == order.Id);
        //            ord.Table.Status = TableStatus.Busy;
        //            await _context.SaveChangesAsync();
        //            //if (int.Parse(userId) == order.UserId)
        //            //{
        //            //    await _hubContext.Clients.User(userId).SendAsync($"Notify", "Поступил заказ");
        //            //}
        //        }
        //        else
        //        {
        //            return BadRequest(new { status = "error", message = "Table is busy or booked"});
        //        }
        //        transaction.Commit();
        //    }
        //    return Ok(new { status = "success", message = "Order was created" });
        //}

        [Route("addMealToOrder")]
        [HttpPost]
        public async Task<IActionResult> AddMealToOrder([FromBody] AddMealToOrderModel model)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order != null)
            {
                order.DateTimeOrdered = DateTime.UtcNow;
                order.OrderStatus = OrderStatus.Active;
                foreach (var item in model.Meals)
                {
                    var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == item.MealId);
                    if (meal != null)
                    {
                        var mealOrderExist = _context.MealOrders.FirstOrDefault(mo => mo.MealId == item.MealId);
                        item.OrderId = model.OrderId;
                        item.MealOrderStatus = MealOrderStatus.NotReady;
                        if (mealOrderExist != null)
                        {
                            mealOrderExist.Quantity += item.Quantity;
                        }
                        else
                        {
                            _context.MealOrders.Add(item);
                        }
                    }
                    else
                    {
                        return NotFound(new { status = "error", message = "Meal was not found" });
                    }
                }
            }
            else
            {
                return NotFound(new { status = "error", message = "Order was not found" });
            }
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { status = "success", message = "Meals was added to order" });
        }
        [Route("closeCheque")]
        [HttpPost]
        public async Task<IActionResult> CloseCheque([FromBody] ChequeModel model)
        {
            if (model.OrderId <= 0)
            {
                return BadRequest(new { status = "error", message = "Json model is not valid" });
            }

            var order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);

            if (order == null)
            {
                return NotFound(new { status = "error", message = "Order was not found"});
            }
            if (order.OrderStatus == OrderStatus.Active || order.OrderStatus == OrderStatus.MealCooked || order.OrderStatus == OrderStatus.BarCooked)
            {
                decimal sum = 0;
                var mealOrders = _context.MealOrders.Where(mo => mo.OrderId == model.OrderId).Select(mo => new
                {
                    meal = mo.Meal,
                    quantity = mo.Quantity
                });
                foreach (var item in mealOrders)
                {
                    var itemPrice = item.meal.Price;
                    var count = item.quantity;
                    var countPrice = itemPrice * count;
                    sum += countPrice;
                }
                order.TotalPrice = sum;
                order.DateTimeClosed = DateTime.UtcNow;
                order.Table.Status = TableStatus.Free;
                order.OrderStatus = OrderStatus.NotActive;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return CreatedAtAction("getOrder", new { id = order.Id }, order);
            }
            return BadRequest(new { status = "error", message = "Order is not active"});
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