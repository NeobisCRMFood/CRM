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
    public class OrderStatusController : ControllerBase
    {
        private readonly EFDbContext _context;

        public OrderStatusController(EFDbContext context)
        {
            _context = context;
        }

        // GET: api/OrderStatus
        [HttpGet]
        public IQueryable GetOrderStatuses()
        {
            var statuses = _context.OrderStatuses.Select(s => new
            {
                id = s.Id,
                name = s.Name
            });
            return statuses;
        }

        // GET: api/OrderStatus/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderStatus = await _context.OrderStatuses.FindAsync(id);

            if (orderStatus == null)
            {
                return NotFound();
            }

            return Ok(orderStatus);
        }

        // PUT: api/OrderStatus/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderStatus([FromRoute] int id, [FromBody] OrderStatus orderStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != orderStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(orderStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusExists(id))
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

        // POST: api/OrderStatus
        [HttpPost]
        public async Task<IActionResult> PostOrderStatus([FromBody] OrderStatus orderStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.OrderStatuses.Add(orderStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrderStatus", new { id = orderStatus.Id }, orderStatus);
        }

        // DELETE: api/OrderStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderStatus = await _context.OrderStatuses.FindAsync(id);
            if (orderStatus == null)
            {
                return NotFound();
            }

            _context.OrderStatuses.Remove(orderStatus);
            await _context.SaveChangesAsync();

            return Ok(orderStatus);
        }

        private bool OrderStatusExists(int id)
        {
            return _context.OrderStatuses.Any(e => e.Id == id);
        }
    }
}