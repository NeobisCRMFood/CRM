using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.ControllerWork
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : BaseController
    {
        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetLogin()
        {
            return Ok($"Ваш логин: {User.Identity.Name}, Id {GetUserId()}");
        }
    }
}
