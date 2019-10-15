using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "waiter")]
    [Route("api/[controller]")]
    [ApiController]
    public class WaiterController : ControllerBase
    {
        private readonly EFDbContext _context;
        public WaiterController(EFDbContext context)
        {
            _context = context;
        }

        [Route("getOrders")]
        [HttpGet]
        public IQueryable GetOrders()
        {
            List<Order> orderList = new List<Order>();
            var orders = _context.Orders
                .Where(o => o.UserId == GetUserId())
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

        [Route("getOrder")]
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
                .Where(t => t.IsBusy == false)
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
            var userId = User.Claims.First(i => i.Type == "UserId").Value;
            order.UserId = int.Parse(userId);
            order.Table.IsBusy = true;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
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
            order.OrderStatusId = 3;
            order.TotalPrice = model.TotalPrice;
            order.DateTimeClosed = DateTime.Now;
            order.Table.IsBusy = false;
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