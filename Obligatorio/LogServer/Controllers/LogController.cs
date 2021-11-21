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

        //https://localhost:44374/logs/

        [HttpGet]
        public async Task<IActionResult> Get([FromBody] string data)
        {
            //List<>;
            if (data == null)
            {
                return BadRequest();
            }
            else
            {
                return Ok();
            }
        }
    }
}
