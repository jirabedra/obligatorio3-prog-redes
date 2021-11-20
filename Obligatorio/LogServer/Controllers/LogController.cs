using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace LogServer.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("logs")]
    [ApiController]
    public class LogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
