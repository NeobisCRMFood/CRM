using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers.ControllerWork
{
    //[Authorize(Roles = "cook")]
    [Route("api/[controller]")]
    [ApiController]
    public class CookController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<FoodHub> _hubContext;
        public CookController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getActiveOrders")]
        [HttpGet]
        public IActionResult ActiveOrders()
        {
            var orders = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Active || o.OrderStatus == OrderStatus.BarCooked)
                .Select(o => new
                {
                    orderId = o.Id,
                    dateTimeOrdered = o.DateTimeOrdered,
                    comment = o.Comment,
                    mealsList = o.MealOrders.Select(mo => new
                    {
                        departmentName = mo.Meal.Category.Department.Name,
                        mealId = mo.MealId,
                        mealName = mo.Meal.Name,
                        quantity = mo.Quantity,
                        statusId = mo.MealOrderStatus,
                        status = mo.MealOrderStatus.ToString()
                    }),
                    orderStatus = o.OrderStatus.ToString()
                }).OrderBy(o => o.dateTimeOrdered);
            return Ok(orders);
        }

        [Route("getMeals")]
        [HttpGet]
        public IActionResult GetMeals()
        {
            var meals = _context.Meals;
            return Ok(meals);
        }

        [Route("changeMealStatus/{id}")]
        [HttpPut]
        public async Task<IActionResult> ChangeMealStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == id);
            if (meal != null)
            {
                if (meal.MealStatus == MealStatus.Have)
                {
                    meal.MealStatus = MealStatus.HaveNot;
                    await _context.SaveChangesAsync();

                    //string message = $"Ингредиентов для блюда {meal.Name} не осталось в наличии";
                    //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(GetUserId()));
                    //if (user.Role == Role.admin || user.Role == Role.cook)
                    //{
                    //    await _hubContext.Clients.User(user.Id.ToString()).SendAsync($"Notify", message);
                    //}
                    return Ok(meal);
                }
                else if (meal.MealStatus == MealStatus.HaveNot)
                {
                    meal.MealStatus = MealStatus.Have;
                    await _context.SaveChangesAsync();

                    //string message = $"Ингредиенты для блюда {meal.Name} появились в наличии";
                    //var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(GetUserId()));
                    //if (user.Role == Role.admin || user.Role == Role.cook)
                    //{
                    //    await _hubContext.Clients.User(user.Id.ToString()).SendAsync($"Notify", message);
                    //}
                    return Ok(meal);
                }
            }
            return NotFound();
        }

        [HttpGet("GetOrder/{id}")]
        private async Task<IActionResult> GetOrder([FromRoute] int id)
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

        [Route("closeMeal")]
        [HttpPost]
        public async Task<IActionResult> CloseMeal([FromBody]MealReadyModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders.Include(mo => mo.MealOrders).FirstOrDefaultAsync(o => o.Id == model.OrderId);
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == model.MealId);

            if (order != null && meal != null)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    var mealOrder = order.MealOrders.FirstOrDefault(mo => mo.MealId == meal.Id);
                    mealOrder.MealOrderStatus = MealOrderStatus.Ready;
                    _context.Entry(mealOrder).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    //var userId = User.Claims.First(i => i.Type == "UserId").Value;
                    //if (int.Parse(userId) == order.UserId)
                    //{
                    //    string message = $"Стол: {order.Table.Name}, Блюдо: {order.MealOrders.Select(mo => mo.Meal.Name)}";
                    //    await _hubContext.Clients.User(userId).SendAsync($"Notify", message);
                    //}
                    transaction.Commit();
                }
                return CreatedAtAction("GetOrder", new { id = order.Id }, order);
            }
            return NotFound();
        }

        [HttpPut("closeOrder/{id}")]
        public async Task<IActionResult> CloseOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                order.OrderStatus = OrderStatus.MealCooked;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetOrder", new { id = order.Id}, order);
            }
            return NotFound();
        }
    }
}