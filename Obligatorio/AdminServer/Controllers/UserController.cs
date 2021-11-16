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

        //https://localhost:44328/users/
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.AddUserAsync(
                              new UserProto()); ;
            return Ok(reply);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.DeleteUserAsync(
                new UserName()
                {
                    Name = id
                });
            return Ok(reply);
        }
    }
}
