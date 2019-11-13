using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Hubs;
using API.Models;
using DataTier.Entities.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.ControllerWork
{
    ///[Authorize(Roles = "barman")]
    [Route("api/[controller]")]
    [ApiController]
    public class BarmanController : ControllerBase
    {
        private readonly EFDbContext _context;
        private readonly IHubContext<FoodHub> _hubContext;

        public BarmanController(EFDbContext context, IHubContext<FoodHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [Route("getActiveOrders")]
        [HttpGet]
        public IActionResult ActiveOrders()
        {
            var orders = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Active || o.OrderStatus == OrderStatus.MealCooked)
                .Select(o => new
                {
                    orderId = o.Id,
                    dateTimeOrdered = o.DateTimeOrdered,
                    comment = o.Comment,
                    mealsList = o.MealOrders.Select(mo => new
                    {
                        departmentName = mo.Meal.Category.Department.ToString(),
                        mealId = mo.MealId,
                        mealName = mo.Meal.Name,
                        quantity = mo.OrderedQuantity,
                        statusId = mo.MealOrderStatus,
                        status = mo.MealOrderStatus.ToString()
                    }),
                    orderStatus = o.OrderStatus.ToString()
                }).OrderBy(o => o.dateTimeOrdered);
            return Ok(orders);
        }

        [Route("closeDrink")]
        [HttpPost]
        public async Task<IActionResult> CloseDrink([FromBody]MealReadyModel model)
        {
            if (model.MealId <= 0 || model.OrderId <= 0)
            {
                return BadRequest(new { status = 400, message = "Json model is not valid"});
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
            return NotFound(new { status = 404, message = "Order or drink was not found"});
        }

        [Route("getMeals")]
        [HttpGet]
        public IActionResult GetMeals()
        {
            var meals = _context.Meals.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                description = m.Description,
                department = m.Category.Department.ToString(),
                category = m.Category.Name,
                price = m.Price,
                weight = m.Weight,
                status = m.MealStatus.ToString(),
                image = m.ImageURL
            });
            return Ok(meals);
        }


        [Route("changeMealStatus/{id}")]
        [HttpPut]
        public async Task<IActionResult> ChangeMealStatus([FromRoute] int id)
        {
            var meal = await _context.Meals.FirstOrDefaultAsync(m => m.Id == id);
            if (meal != null)
            {
                if (meal.MealStatus == MealStatus.Have)
                {
                    meal.MealStatus = MealStatus.HaveNot;
                    await _context.SaveChangesAsync();
                    //string message = $"Ингредиенты для блюда {meal.Name} появились в наличии";
                    //var userId = GetUserId();
                    //await _hubContext.Clients.User(userId).SendAsync($"Notify", message);
                    return Ok(meal);
                }
                else if (meal.MealStatus == MealStatus.HaveNot)
                {
                    meal.MealStatus = MealStatus.Have;
                    await _context.SaveChangesAsync();
                    //string message = $"Ингредиентов для блюда {meal.Name} не осталось в наличии";
                    //var userId = GetUserId();
                    //await _hubContext.Clients.User(userId).SendAsync($"Notify", message);
                    return Ok(meal);
                }
            }
            return NotFound(new { status = 404, message = "Meal was not Found" });
        }

        [HttpPut("closeOrder/{id}")]
        public async Task<IActionResult> CloseOrder([FromRoute] int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                order.OrderStatus = OrderStatus.BarCooked;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetOrder", new { id = order.Id }, order);
            }
            return NotFound(new { status = 404, message = "Order was not Found" });
        }
    }
}