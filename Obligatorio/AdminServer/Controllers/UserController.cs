using AdminServer.Services;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminServer.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("users")]
    [ApiController]
    public class UserController : Controller
    {
        private static ClientUserService userService = new ClientUserService();

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.AddUserAsync(
                              new UserProto ()); ;
            return Ok(reply);
        }


    }
}
