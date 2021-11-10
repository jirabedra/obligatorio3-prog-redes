using AdminServer.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminServer.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("games")]
    [ApiController]
    public class GameController : Controller
    {
        private static GameService gameService = new GameService();

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            var res = Task.Run(async () => await gameService.GetGame(name)).Result;
            return Ok(res);
        }
    }
}
