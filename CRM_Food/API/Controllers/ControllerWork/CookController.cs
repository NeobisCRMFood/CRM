using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "cook")]
    [Route("api/[controller]")]
    [ApiController]
    public class CookController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        public CookController(EFDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getActiveOrders")]
        [HttpGet]
        public IQueryable ActiveOrders()
        {
            var orders = _context.Orders.Where(o => o.OrderStatusId == 1).Select(o => new
            {
                id = o.Id,
                dateTimeOrdered = o.DateTimeOrdered,
                comment = o.Comment,
                mealsList = o.MealOrders.Select(mo => new
                {
                    mealId = mo.MealId,
                    mealName = mo.Meal.Name,
                    quantity = mo.Quantity,
                    status = mo.MealOrderStatus.Name
                }),
                OrderStatus = o.OrderStatus.Name,
            }).OrderBy(o => o.dateTimeOrdered);
            return orders;
        }

        [Route("closeMeal")]
        [HttpPost]
        public async Task<IActionResult> CloseMeal([FromBody]MealReady model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.Include(mo => mo.MealOrders).FirstOrDefaultAsync(o => o.Id == model.OrderId);
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == model.MealId);

            if (order != null && meal != null)
            {
                var mealOrder = order.MealOrders.FirstOrDefault(mo => mo.MealId == meal.Id);
                mealOrder.MealOrderStatusId = 2;
                _context.Entry(mealOrder).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var userId = User.Claims.First(i => i.Type == "UserId").Value;
                if (int.Parse(userId) == order.UserId)
                {
                    string message = $"Стол: {order.Table.Name}, Блюдо: {order.MealOrders.Select(mo => mo.Meal.Name)}";
                    await _hubContext.Clients.User(userId).SendAsync($"Notify", message);
                }
            }
            return NoContent();
        }
    }
}