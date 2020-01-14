using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using API.Week;
using DataTier.Entities.Abstract;
using DataTier.Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers.ControllerWork
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private EFDbContext _context;
        public StatisticController(EFDbContext context)
        {
            _context = context;
        }
        [Route("waiter/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetWaiterStatistics([FromRoute] int id)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "User is not waiter" });
            }

            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .Sum()
            });

            var statiscticsToday = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
               .Where(o => o.DateTimeClosed >= DateTime.Today)
               .Count(),

                totalSum = u.Orders
               .Where(o => o.OrderStatus == OrderStatus.NotActive)
               .Where(o => o.DateTimeClosed >= DateTime.Today)
               .Select(o => o.TotalPrice)
               .Sum()
            });

            var statiscticsWeek = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Select(o => o.TotalPrice)
                .Sum()
            });

            var statiscticsMonth = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .Sum()
            });

            return Ok(new { statisctics, statiscticsToday, statiscticsWeek, statiscticsMonth });
        }

        [Route("totalSums")]
        [HttpGet]
        public async Task<IActionResult> TotalSum()
        {
            var totalSum = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumToday = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Today)
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumLastDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-2) && o.DateTimeClosed <= DateTime.Now.AddDays(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumLastWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-14) && o.DateTimeClosed <= DateTime.Now.AddDays(-7))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalSumLastMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-2) && o.DateTimeClosed <= DateTime.Now.AddMonths(-1))
                .Select(o => o.TotalPrice)
                .SumAsync();

            var totalPrices = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice)
                .Sum();
            int count = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Count();
            decimal totalSumAverage;
            if (count == 0)
            {
                totalSumAverage = 0;
            }
            else
            {
                totalSumAverage = totalPrices / count;
            }

            return Ok(new
            {
                totalSum,
                totalSumToday,
                totalSumMonth,
                totalSumLastMonth,
                totalSumWeek,
                totalSumLastWeek,
                totalSumDay,
                totalSumLastDay,
                totalSumAverage
            });
        }

        [Route("totalSumsYear")]
        [HttpGet]
        public async Task<IActionResult> TotalSumYear()
        {
            var year = DateTime.Now.Year;
            var january = new DateTime(year, 1, 1);
            var januaryEnd = new DateTime(year, january.Month, DateTime.DaysInMonth(year, january.Month));
            var januarySum = await _context.Orders.Where(o => o.DateTimeClosed >= january && o.DateTimeClosed <= januaryEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var february = new DateTime(year, 2, 1);
            var februaryEnd = new DateTime(year, february.Month, DateTime.DaysInMonth(year, february.Month));
            var februarySum = await _context.Orders.Where(o => o.DateTimeClosed >= february && o.DateTimeClosed <= februaryEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var march = new DateTime(year, 3, 1);
            var marchEnd = new DateTime(year, march.Month, DateTime.DaysInMonth(year, march.Month));
            var marchSum = await _context.Orders.Where(o => o.DateTimeClosed >= march && o.DateTimeClosed <= marchEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var april = new DateTime(year, 4, 1);
            var aprilEnd = new DateTime(year, april.Month, DateTime.DaysInMonth(year, april.Month));
            var aprilSum = await _context.Orders.Where(o => o.DateTimeClosed >= april && o.DateTimeClosed <= aprilEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var may = new DateTime(year, 5, 1);
            var mayEnd = new DateTime(year, may.Month, DateTime.DaysInMonth(year, may.Month));
            var maySum = await _context.Orders.Where(o => o.DateTimeClosed >= may && o.DateTimeClosed <= mayEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var june = new DateTime(year, 6, 1);
            var juneEnd = new DateTime(year, june.Month, DateTime.DaysInMonth(year, june.Month));
            var juneSum = await _context.Orders.Where(o => o.DateTimeClosed >= june && o.DateTimeClosed <= juneEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var july = new DateTime(year, 7, 1);
            var julyEnd = new DateTime(year, july.Month, DateTime.DaysInMonth(year, july.Month));
            var julySum = await _context.Orders.Where(o => o.DateTimeClosed >= july && o.DateTimeClosed <= julyEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var august = new DateTime(year, 8, 1);
            var augustEnd = new DateTime(year, august.Month, DateTime.DaysInMonth(year, august.Month));
            var augustSum = await _context.Orders.Where(o => o.DateTimeClosed >= august && o.DateTimeClosed <= augustEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var september = new DateTime(year, 9, 1);
            var septemberEnd = new DateTime(year, september.Month, DateTime.DaysInMonth(year, september.Month));
            var septemberSum = await _context.Orders.Where(o => o.DateTimeClosed >= september && o.DateTimeClosed <= septemberEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var october = new DateTime(year, 10, 1);
            var octoberEnd = new DateTime(year, october.Month, DateTime.DaysInMonth(year, october.Month));
            var octoberSum = await _context.Orders.Where(o => o.DateTimeClosed >= october && o.DateTimeClosed <= octoberEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var november = new DateTime(year, 11, 1);
            var novemberEnd = new DateTime(year, november.Month, DateTime.DaysInMonth(year, november.Month));
            var novemberSum = await _context.Orders.Where(o => o.DateTimeClosed >= november && o.DateTimeClosed <= novemberEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var december = new DateTime(year, 12, 1);
            var decemberEnd = new DateTime(year, december.Month, DateTime.DaysInMonth(year, december.Month));
            var decemberSum = await _context.Orders.Where(o => o.DateTimeClosed >= december && o.DateTimeClosed <= decemberEnd).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            return Ok(new
            {
                januarySum,
                februarySum,
                marchSum,
                aprilSum,
                maySum,
                juneSum,
                julySum,
                augustSum,
                septemberSum,
                octoberSum,
                novemberSum,
                decemberSum
            });
        }

        [Route("totalSumsMonth")]
        [HttpGet]
        public async Task<IActionResult> TotalSumsMonthes()
        {
            var year = DateTime.Now.Year;
            var month = new DateTime(year, DateTime.Now.Month, 1);
            var lastMonth = new DateTime(year, DateTime.Now.Month - 1, 1);
            var week1 = await _context.Orders.Where(o => o.DateTimeClosed >= month && o.DateTimeClosed <= month.AddDays(6)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var week2 = await _context.Orders.Where(o => o.DateTimeClosed >= month.AddDays(6) && o.DateTimeClosed <= month.AddDays(13)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var week3 = await _context.Orders.Where(o => o.DateTimeClosed >= month.AddDays(13) && o.DateTimeClosed <= month.AddDays(20)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var week4 = await _context.Orders.Where(o => o.DateTimeClosed >= month.AddDays(20) && o.DateTimeClosed <= month.AddDays(27)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var monthEnd = await _context.Orders.Where(o => o.DateTimeClosed >= month.AddDays(27) && o.DateTimeClosed <= new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month))).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();


            var lastWeek1 = await _context.Orders.Where(o => o.DateTimeClosed >= lastMonth && o.DateTimeClosed <= lastMonth.AddDays(6)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var lastWeek2 = await _context.Orders.Where(o => o.DateTimeClosed >= lastMonth.AddDays(6) && o.DateTimeClosed <= lastMonth.AddDays(13)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var lastWeek3 = await _context.Orders.Where(o => o.DateTimeClosed >= lastMonth.AddDays(13) && o.DateTimeClosed <= lastMonth.AddDays(20)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var lastWeek4 = await _context.Orders.Where(o => o.DateTimeClosed >= lastMonth.AddDays(20) && o.DateTimeClosed <= lastMonth.AddDays(27)).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            var lastMonthEnd = await _context.Orders.Where(o => o.DateTimeClosed >= lastMonth.AddDays(27) && o.DateTimeClosed <= new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month))).Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => o.TotalPrice).SumAsync();

            return Ok(new { LastMonth = new { lastWeek1, lastWeek2, lastWeek3, lastWeek4, lastMonthEnd }, Month = new { week1, week2, week3, week4, monthEnd } });
        }

        [Route("totalSumsWeek")]
        [HttpGet]
        public async Task<IActionResult> TotalSumsWeek()
        {
            var firstDayWeek = DateTimeExtensions.FirstDayOfWeek(DateTime.Now);
            var firstDayLastWeek = DateTimeExtensions.FirstDayOfWeek(DateTime.Now).AddDays(-7);
            var day1 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek && o.DateTimeClosed <= firstDayWeek.AddDays(1)).Select(o => o.TotalPrice).SumAsync();

            var day2 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek.AddDays(1) && o.DateTimeClosed <= firstDayWeek.AddDays(2)).Select(o => o.TotalPrice).SumAsync();

            var day3 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek.AddDays(2) && o.DateTimeClosed <= firstDayWeek.AddDays(3)).Select(o => o.TotalPrice).SumAsync();

            var day4 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek.AddDays(3) && o.DateTimeClosed <= firstDayWeek.AddDays(4)).Select(o => o.TotalPrice).SumAsync();

            var day5 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek.AddDays(4) && o.DateTimeClosed <= firstDayWeek.AddDays(5)).Select(o => o.TotalPrice).SumAsync();

            var day6 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek.AddDays(5) && o.DateTimeClosed <= firstDayWeek.AddDays(6)).Select(o => o.TotalPrice).SumAsync();

            var day7 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayWeek.AddDays(6) && o.DateTimeClosed <= firstDayWeek.AddDays(7)).Select(o => o.TotalPrice).SumAsync();


            var lastDay1 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek && o.DateTimeClosed <= firstDayLastWeek.AddDays(1)).Select(o => o.TotalPrice).SumAsync();

            var lastDay2 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek.AddDays(1) && o.DateTimeClosed <= firstDayLastWeek.AddDays(2)).Select(o => o.TotalPrice).SumAsync();

            var lastDay3 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek.AddDays(2) && o.DateTimeClosed <= firstDayLastWeek.AddDays(3)).Select(o => o.TotalPrice).SumAsync();

            var lastDay4 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek.AddDays(3) && o.DateTimeClosed <= firstDayLastWeek.AddDays(4)).Select(o => o.TotalPrice).SumAsync();

            var lastDay5 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek.AddDays(4) && o.DateTimeClosed <= firstDayLastWeek.AddDays(5)).Select(o => o.TotalPrice).SumAsync();

            var lastDay6 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek.AddDays(5) && o.DateTimeClosed <= firstDayLastWeek.AddDays(6)).Select(o => o.TotalPrice).SumAsync();

            var lastDay7 = await _context.Orders.Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= firstDayLastWeek.AddDays(6) && o.DateTimeClosed <= firstDayLastWeek.AddDays(7)).Select(o => o.TotalPrice).SumAsync();

            return Ok(new { LastWeek = new { lastDay1, lastDay2, lastDay3, lastDay4, lastDay5, lastDay6, lastDay7 }, Week = new { day1, day2, day3, day4, day5, day6, day7 } });
        }
        [Route("totalOrders")]
        [HttpGet]
        public async Task<IActionResult> TotalOrders()
        {
            var totalOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .CountAsync();

            var totalOrdersToday = await _context.Orders
               .Where(o => o.OrderStatus == OrderStatus.NotActive)
               .Where(o => o.DateTimeClosed >= DateTime.Today)
               .CountAsync();

            var totalOrdersDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .CountAsync();

            var totalOrdersLastDay = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-2) && o.DateTimeClosed <= DateTime.Now.AddDays(-1))
                .CountAsync();

            var totalOrdersWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .CountAsync();

            var totalOrdersLastWeek = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddDays(-14) && o.DateTimeClosed <= DateTime.Now.AddDays(-7))
                .CountAsync();

            var totalOrdersMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .CountAsync();

            var totalOrdersLastMonth = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= DateTime.Now.AddMonths(-2) && o.DateTimeClosed <= DateTime.Now.AddMonths(-1))
                .CountAsync();

            return Ok(new
            {
                totalOrders,
                totalOrdersToday,
                totalOrdersMonth,
                totalOrdersLastMonth,
                totalOrdersWeek,
                totalOrdersLastWeek,
                totalOrdersDay,
                totalOrdersLastDay
            });
        }

        [Route("kitchenTotalSums")]
        [HttpGet]
        public async Task<IActionResult> KitchenTotalSums()
        {
            var totalSum = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .SumAsync();

            var totalSumMonth = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .SumAsync();

            var totalSumWeek = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .SumAsync();

            var totalSumDay = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .SumAsync();

            return Ok(new
            {
                totalSum,
                totalSumMonth,
                totalSumWeek,
                totalSumDay
            });
        }

        [Route("kitchenTotalMeals")]
        [HttpGet]
        public async Task<IActionResult> KitchenTotalMeals()
        {
            var totalMeals = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            var totalMealsMonth = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            var totalMealsWeek = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            var totalMealsToday = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            return Ok(new
            {
                totalMeals,
                totalMealsMonth,
                totalMealsWeek,
                totalMealsToday
            });
        }

        [Route("barTotalMeals")]
        [HttpGet]
        public async Task<IActionResult> BarTotalMeals()
        {
            var totalMeals = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            var totalMealsMonth = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            var totalMealsWeek = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            var totalMealsToday = await _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeOrdered >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.FinishedQuantity).SumAsync();

            return Ok(new
            {
                totalMeals,
                totalMealsMonth,
                totalMealsWeek,
                totalMealsToday
            });
        }

        [Route("barTotalSums")]
        [HttpGet]
        public IActionResult BarTotalSums()
        {
            var totalSum = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .Sum();

            var totalSumMonth = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddMonths(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .Sum();

            var totalSumWeek = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-7))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .Sum();

            var totalSumDay = _context.MealOrders
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Order.DateTimeClosed >= DateTime.Now.AddDays(-1))
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity)
                .Sum();

            return Ok(new { totalSum, totalSumMonth, totalSumWeek, totalSumDay });
        }

        [Route("topMeals")]
        [HttpGet]
        public IActionResult SalesByMeal()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.MealId == m.Id)
                    .Select(mo => mo.FinishedQuantity)
                    .Sum()
                })
                .Take(10)
                .OrderByDescending(m => m.count);
            return Ok(meals);
        }

        #region Топ блюд по сезонам

        [Route("topMealsWinter")]
        [HttpGet]
        public IActionResult SalesByMealWinter()
        {
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                var leapYearMeals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 29))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
                return Ok(leapYearMeals);
            }

            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 28))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topMealsSpring")]
        [HttpGet]
        public IActionResult SalesByMealSpring()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 3, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 5, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("topMealsSummer")]
        [HttpGet]
        public IActionResult SalesByMealSummer()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 6, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 9, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topMealsAutumn")]
        [HttpGet]
        public IActionResult SalesByMealAutumn()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 10, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 11, 30))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        #endregion

        [Route("topDrinks")]
        [HttpGet]
        public IActionResult SalesByDrink()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.MealId == m.Id)
                    .Select(mo => mo.FinishedQuantity)
                    .Sum()
                })
                .Take(10)
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        #region Топ алкоголя по сезонам

        [Route("topDrinksWinter")]
        [HttpGet]
        public IActionResult SalesByMealDrinks()
        {
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                var leapYearMeals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 29))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
                return Ok(leapYearMeals);
            }

            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.AddYears(-1).Year, 12, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 2, 28))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topDrinksSpring")]
        [HttpGet]
        public IActionResult SalesByDrinksSpring()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 3, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 5, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(meals);
        }

        [Route("topDrinksSummer")]
        [HttpGet]
        public IActionResult SalesByDrinksSummer()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 6, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 9, 31))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        [Route("topDrinksAutumn")]
        [HttpGet]
        public IActionResult SalesByDrinksAutumn()
        {
            var meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.DateTimeClosed >= new DateTime(DateTime.Now.Year, 10, 1) && mo.Order.DateTimeClosed <= new DateTime(DateTime.Now.Year, 11, 30))
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count).Take(10);
            return Ok(meals);
        }

        #endregion

        [Route("transactionHistory")]
        [HttpGet]
        public IEnumerable<Order> LatestOrders([FromQuery] PaginationModel model)
        {
            var source = (from order in _context.Orders.Include(o => o.MealOrders).
                    OrderByDescending(o => o.DateTimeOrdered)
                          select order).AsQueryable();

            int count = source.Count();

            int CurrentPage = model.PageNumber;

            int PageSize = model.PageSize;

            int TotalCount = count;

            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            var items = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

            var previousPage = CurrentPage > 1 ? "Yes" : "No";

            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

            var paginationMetadata = new
            {
                totalCount = TotalCount,
                pageSize = PageSize,
                currentPage = CurrentPage,
                totalPages = TotalPages,
                previousPage,
                nextPage
            };

            HttpContext.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));

            return items;
        }

        [Route("bestWaiterLastMonth")]
        [HttpGet]
        public IActionResult BestWaiter()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            if (users == null)
            {
                return BadRequest(new { status = "error", message = "Have no waiters" });
            }
            DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
            DateTime finish = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1));
            var top = users.Select(u => new
            {
                id = u.Id,
                name = u.LastName + " " + u.FirstName,
                orderCount = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= start && o.DateTimeClosed <= finish)
                .Count()
            })
            .OrderByDescending(u => u.orderCount).Take(1);

            return Ok(top);
        }

        [Route("waiterOrderTop")]
        [HttpGet]
        public IActionResult WaiterTop()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            if (users == null)
            {
                return BadRequest(new { status = "error", message = "Have no waiters" });
            }
            var top = users.Select(u => new
            {
                id = u.Id,
                name = u.LastName + " " + u.FirstName,
                orderCount = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Count()
            })
            .OrderByDescending(u => u.orderCount);

            return Ok(top);
        }

        [Route("waiterSumTop")]
        [HttpGet]
        public IActionResult WaiterSumTop()
        {
            var waitersSumTop = _context.Users
                .Where(u => u.Role == Role.waiter)
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.LastName + " " + u.FirstName,
                    sum = _context.MealOrders.Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => mo.Order.TotalPrice)
                    .Sum()
                }).OrderBy(mo => mo.sum);
            return Ok(waitersSumTop);
        }

        [Route("kitchenOrders")]
        [HttpGet]
        public IActionResult KitchenOrdersStatistics()
        {
            var ordersTop = _context.Meals
                .Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(ordersTop);
        }

        [Route("kitchenSum")]
        [HttpGet]
        public IActionResult KitchenSumStatistics()
        {
            var sumTop = _context.Meals
               .Where(m => m.Category.Department == Department.Kitchen)
               .Select(m => new
               {
                   m.Id,
                   m.Name,
                   m.Price,
                   quantity = m.MealOrders
                   .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                   .Where(mo => mo.MealId == m.Id)
                   .Select(mo => mo.FinishedQuantity)
                   .Sum()
               })
               .OrderByDescending(mo => mo.quantity);
            return Ok(sumTop);
        }

        [Route("kitchenWaiterMeals")]
        [HttpGet]
        public IActionResult KitchenWaiterMealStatistics()
        {
            var waitersMealTop = _context.Users
                .Where(u => u.Role == Role.waiter)
                .Select(u => new
                {
                    userId = u.Id,
                    userName = u.LastName + " " + u.FirstName,
                    meals = _context.Meals.Where(m => m.Category.Department == Department.Kitchen)
                    .Select(m => new
                    {
                        mealId = m.Id,
                        mealName = m.Name,
                        count = m.MealOrders
                        .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                        .Where(mo => mo.Order.UserId == u.Id)
                        .Select(mo => new
                        {
                            mo.OrderId
                        })
                    .Count()
                    }).OrderByDescending(mo => mo.count)
                });
            return Ok(waitersMealTop);
        }

        [Route("kitchenWaiterSums")]
        [HttpGet]
        public IActionResult KitchenWaiterSumStatistics()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var waitersSumTop = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.Meals.Where(m => m.Category.Department == Department.Kitchen)
                .Select(m => new
                {
                    mealId = m.Id,
                    mealName = m.Name,
                    sum = m.Price * m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => new
                    {
                        mo.OrderId
                    }).Count()
                }).OrderByDescending(mo => mo.sum)
            });
            return Ok(waitersSumTop);
        }

        [Route("barOrders")]
        [HttpGet]
        public IActionResult BarOrdersStatistics()
        {
            var ordersTop = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Select(mo => new
                    {
                        mo.OrderId
                    })
                .Count()
                })
            .OrderByDescending(mo => mo.count);
            return Ok(ordersTop);
        }

        [Route("barSums")]
        [HttpGet]
        public IActionResult BarSumStatistics()
        {
            var sumTop = _context.Meals
               .Where(m => m.Category.Department == Department.Bar)
               .Select(m => new
               {
                   m.Id,
                   m.Name,
                   m.Price,
                   quantity = m.MealOrders
                   .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                   .Where(mo => mo.MealId == m.Id)
                   .Select(mo => mo.FinishedQuantity)
                   .Sum()
               })
               .OrderByDescending(mo => mo.quantity);
            return Ok(sumTop);
        }

        [Route("barWaiterMeals")]
        [HttpGet]
        public IActionResult BarWaiterMealStatistics()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var waiterMealTop = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    mealId = m.Id,
                    mealName = m.Name,
                    count = m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => new
                    {
                        mo.OrderId
                    }).Count()
                }).OrderByDescending(mo => mo.count)
            });
            return Ok(waiterMealTop);
        }

        [Route("barWaiterSums")]
        [HttpGet]
        public IActionResult BarWaiterSumStatistics()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var waitersSumTop = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.Meals
                .Where(m => m.Category.Department == Department.Bar)
                .Select(m => new
                {
                    mealId = m.Id,
                    mealName = m.Name,
                    sum = m.Price * m.MealOrders
                    .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                    .Where(mo => mo.Order.UserId == u.Id)
                    .Select(mo => new
                    {
                        mo.OrderId
                    }).Count()
                }).OrderByDescending(mo => mo.sum)
            });
            return Ok(waitersSumTop);
        }

        [Route("lastOrders")]
        [HttpGet]
        public IActionResult LastOrders()
        {
            var orders = _context.Orders.OrderByDescending(o => o.DateTimeOrdered).Select(o => new
            {
                orderId = o.Id,
                waiterName = o.User.FirstName + " " + o.User.LastName,
                orderDate = o.DateTimeOrdered,
                status = o.OrderStatus.ToString(),
                totalPrice = o.TotalPrice,
                mealOrders = o.MealOrders.Select(mo => new
                {
                    mo.Meal.Name,
                    mo.OrderedQuantity
                })
            }).Take(5);
            return Ok(orders);
        }
        [Route("lastFinishedOrders")]
        [HttpGet]
        public IActionResult LastFinishedOrders()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.DateTimeClosed)
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Select(o => new
                {
                    orderId = o.Id,
                    waiterName = o.User.FirstName + " " + o.User.LastName,
                    orderDate = o.DateTimeOrdered,
                    status = o.OrderStatus.ToString(),
                    totalPrice = o.TotalPrice,
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        mo.Meal.Name,
                        mo.OrderedQuantity
                    })
                }).Take(5);
            return Ok(orders);
        }

        [Route("topWaitersKitchenSums")]
        [HttpGet]
        public IActionResult TopWaitersKitchenTotalSums()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                sum = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.sum);
            return Ok(topWaiters);
        }

        [Route("topWaitersBarSums")]
        [HttpGet]
        public IActionResult TopWaitersBarTotalSums()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                sum = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.Meal.Price * mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.sum);
            return Ok(topWaiters);
        }

        [Route("topWaitersKitchenMeals")]
        [HttpGet]
        public IActionResult TopWaitersKitchenTotalMeals()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Kitchen)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.meals);
            return Ok(topWaiters);
        }

        [Route("topWaitersBarMeals")]
        [HttpGet]
        public IActionResult TopWaitersBarTotalMeals()
        {
            var users = _context.Users.Where(u => u.Role == Role.waiter);
            var topWaiters = users.Select(u => new
            {
                userId = u.Id,
                userName = u.LastName + " " + u.FirstName,
                meals = _context.MealOrders
                .Include(mo => mo.Order)
                .Include(mo => mo.Meal)
                .Where(mo => mo.Order.OrderStatus == OrderStatus.NotActive)
                .Where(mo => mo.Meal.Category.Department == Department.Bar)
                .Where(mo => mo.Order.UserId == u.Id)
                .Select(mo => mo.FinishedQuantity).Sum()
            }).OrderBy(u => u.meals);
            return Ok(topWaiters);
        }

        [Route("waiterStatisticsDateRange/{id}")]
        [HttpPost]
        public async Task<IActionResult> GetWaiterStatisticsRange([FromRoute] int id, [FromBody] DateRange model)
        {
            var waiter = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (waiter == null)
            {
                return NotFound(new { status = "error", message = "Официант не был найден" });
            }

            if (waiter.Role != Role.waiter)
            {
                return BadRequest(new { status = "error", message = "Пользователь не является официантом" });
            }

            var statisctics = _context.Users.Where(u => u.Id == waiter.Id).Select(u => new
            {
                orderCount = u.Orders
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeClosed <= o.DateTimeClosed)
                .Count(),

                totalSum = u.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .Select(o => o.TotalPrice)
                .Sum()
            });
            return Ok(statisctics);
        }

        [Route("transactionHistoryDateRange")]
        [HttpPost]
        public IActionResult LatestOrdersRange(DateRange model)
        {
            var orders = _context.Orders
                .Where(o => o.DateTimeOrdered >= model.StartDate && o.DateTimeOrdered <= model.EndDate)
                .OrderByDescending(o => o.DateTimeOrdered)
                .Select(o => new
                {
                    orderId = o.Id,
                    waiterName = o.User.FirstName + " " + o.User.LastName,
                    orderDate = o.DateTimeOrdered,
                    status = o.OrderStatus.ToString(),
                    totalPrice = o.TotalPrice,
                    mealOrders = o.MealOrders.Select(mo => new
                    {
                        mo.Meal.Name,
                        mo.OrderedQuantity
                    })
                });
            return Ok(orders);
        }

        [Route("totalSumDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalSumDateRange([FromBody] DateRange model)
        {
            var totalPrices = _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .Select(o => o.TotalPrice);
            var sum = await totalPrices.SumAsync();
            return Ok(sum);
        }
        [Route("totalOrdersDateRange")]
        [HttpPost]
        public async Task<IActionResult> TotalOrdersDateRange([FromBody] DateRange model)
        {
            var totalPrices = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.NotActive)
                .Where(o => o.DateTimeClosed >= model.StartDate && o.DateTimeClosed <= model.EndDate)
                .CountAsync();
            return Ok(totalPrices);
        }

        [Route("getBooksDateRange")]
        [HttpPost]
        public IActionResult BooksDateRange([FromBody] DateTime date)
        {
            DateTime dayEnd = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            var books = _context.Books.Where(b => b.BookDate >= date && b.BookDate <= dayEnd)
                .Select(b => new
                {
                    b.Id,
                    b.ClientName,
                    b.MenQuantity,
                    b.PhoneNumber,
                    b.BookDate,
                    b.TableId
                });
            return Ok(books);
        }

    }
}