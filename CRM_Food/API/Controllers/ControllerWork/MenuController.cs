using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTier.Entities.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.ControllerWork.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private EFDbContext _context;

        public MenuController(EFDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get_menu()
        {
            var menu = _context.Categories.Include(c => c.Meals);
            return Ok(menu);
        }
    }
}