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
    public class MealOrderStatusController : ControllerBase
    {
        private readonly EFDbContext _context;

        public MealOrderStatusController(EFDbContext context)
        {
            _context = context;
        }

        // GET: api/MealOrderStatus
        [HttpGet]
        public IQueryable GetMealOrderStatuses()
        {
            var statuses = _context.MealOrderStatuses.Select(s => new
            {
                id = s.Id,
                name = s.Name
            });
            return statuses;
        }

        // GET: api/MealOrderStatus/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMealOrderStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mealOrderStatus = await _context.MealOrderStatuses.FindAsync(id);

            if (mealOrderStatus == null)
            {
                return NotFound();
            }

            return Ok(mealOrderStatus);
        }

        // PUT: api/MealOrderStatus/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMealOrderStatus([FromRoute] int id, [FromBody] MealOrderStatus mealOrderStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mealOrderStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(mealOrderStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MealOrderStatusExists(id))
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

        // POST: api/MealOrderStatus
        [HttpPost]
        public async Task<IActionResult> PostMealOrderStatus([FromBody] MealOrderStatus mealOrderStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.MealOrderStatuses.Add(mealOrderStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMealOrderStatus", new { id = mealOrderStatus.Id }, mealOrderStatus);
        }

        // DELETE: api/MealOrderStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMealOrderStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var mealOrderStatus = await _context.MealOrderStatuses.FindAsync(id);
            if (mealOrderStatus == null)
            {
                return NotFound();
            }

            _context.MealOrderStatuses.Remove(mealOrderStatus);
            await _context.SaveChangesAsync();

            return Ok(mealOrderStatus);
        }

        private bool MealOrderStatusExists(int id)
        {
            return _context.MealOrderStatuses.Any(e => e.Id == id);
        }
    }
}