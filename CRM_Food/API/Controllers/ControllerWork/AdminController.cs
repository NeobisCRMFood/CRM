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

        [Route("getWaiters")]
        [HttpGet]
        public IActionResult GetWaiters()
        {
            var waiters = _context.Users.Where(u => u.RoleId == 2).Select(u => new
            {
                name = u.LastName + " " + u.FirstName + " " + u.MiddleName,
                login = u.Login,
                password = u.Password
            });
            return Ok(waiters);
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
        [Route("TotalSum")]
        [HttpGet]
        public decimal TotalSum()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatusId == 2).Select(o => o.TotalPrice);
            var sum = totalPrices.Sum();
            return sum;
        }

        [Route("TotalSumToday")]
        [HttpGet]
        public decimal TotalSumToday()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatusId == 2).Where(o => o.DateTimeClosed >= DateTime.Today).Select(o => o.TotalPrice);
            var sum = totalPrices.Sum();
            return sum;
        }

        [Route("TotalSumAverage")]
        [HttpGet]
        public decimal TotalSumAverage()
        {
            var totalPrices = _context.Orders.Where(o => o.OrderStatusId == 2).Select(o => o.TotalPrice);
            var sum = totalPrices.Sum();
            var count = _context.Orders.Where(o => o.OrderStatusId == 2).Count();
            var totalSum = sum / count;
            return totalSum;
        }
    }
}