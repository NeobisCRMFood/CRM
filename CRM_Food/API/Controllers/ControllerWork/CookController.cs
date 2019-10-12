using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.ControllerWork
{
    [Route("api/[controller]")]
    [ApiController]
    public class CookController : ControllerBase
    {
        private EFDbContext _context;

        public CookController(EFDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IQueryable ActiveOrders()
        {
            var orders = _context.Orders.Where(o => o.OrderStatusId == 1).Select(o => new
            {
                Id = o.Id,
                DateTimeOrdered = o.DateTimeOrdered,
                Comment = o.Comment,
                MealsList = o.MealOrders.Select(mo => new
                {
                    mealName = mo.Meal.Name,
                    status = mo.MealOrderStatus.Name
                }),
                OrderStatus = o.OrderStatus.Name,
            }).OrderBy(o => o.DateTimeOrdered);
            return orders;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> CloseOrder([FromRoute]int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return BadRequest();
            }
            order.OrderStatusId = 2;
            order.DateTimeClosed = DateTime.Now;
            _context.Entry(order).State = EntityState.Modified;
            try
            {
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
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}