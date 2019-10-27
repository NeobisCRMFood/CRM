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
        public IQueryable GetOrders()
        {
            var orders = _context.Orders
                //.Where(o => o.UserId == GetUserId())
                .Where(o => o.OrderStatusId == 1)
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
            return orders;
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
        public IQueryable GetTables()
        {
            var tables = _context.Tables
                .Select(t => new { id = t.Id, name = t.Name });
            return tables;
        }

        [Route("getFreeTables")]
        [HttpGet]
        public IQueryable GetFreeTables()
        {
            var tables = _context.Tables
                .Where(t => t.Status == TableStatus.Free)
                .Select(t => new { id = t.Id, name = t.Name});
            return tables;
        }

        [Route("getMenu")]
        [HttpGet]
        public IQueryable Get_menu()
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
                            price = m.Price
                        })
                    })
                });
            return menu;
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
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                var ord = await _context.Orders.Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == order.Id);
                ord.Table.Status = TableStatus.Busy;
                await _context.SaveChangesAsync();
                //var userId = User.Claims.First(i => i.Type == "UserId").Value;
                //if (int.Parse(userId) == ord.UserId)
                //{
                //    await _hubContext.Clients.User(userId).SendAsync($"Notify", "Поступил заказ");
                //}
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
            Order order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == model.OrderId);
            if (order == null)
            {
                return BadRequest();
            }
            order.OrderStatusId = 2;
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