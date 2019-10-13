using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly EFDbContext _context;

        public OrdersController(EFDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IQueryable GetOrders()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Table)
                .Include(o => o.OrderStatus)
                .Include(o => o.MealOrders)
                .ThenInclude(mo => mo.Meal)
                .Select(o => new
                {
                    id = o.Id,
                    userId = o.UserId,
                    userName = o.User.FirstName + " " + o.User.LastName,
                    tableId = o.TableId,
                    table = o.Table.Name,
                    dateTimeOrdered = o.DateTimeOrdered.ToShortTimeString(),
                    dateTimeClosed = o.DateTimeClosed,
                    orderStatusId = o.OrderStatusId,
                    orderStatus = o.OrderStatus.Name,
                    totalPrice = o.TotalPrice,
                    comment = o.Comment,
                    meals = o.MealOrders
                });
            return orders;
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder([FromRoute] int id, [FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }
            
            _context.Entry(order).State = EntityState.Modified;

            try
            {
                order.DateTimeOrdered = DateTime.Today;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}