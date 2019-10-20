using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTier.Entities.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    }
}