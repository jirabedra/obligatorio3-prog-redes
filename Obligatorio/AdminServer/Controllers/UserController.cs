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

        //https://localhost:44328/users/
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.AddUserAsync(
                              new UserProto()); ;
            if(reply.Result == true)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        //Si existe el usuario con el id indicado, se borra y retorna OK200.
        //Si no lo encuentra retorna 404NOTFOUND
        //https://localhost:44328/users/3
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
            if (reply.Result == true)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
            return Ok(reply);
        }

        //name es el int ya registrado es para apuntar al usuario correcto
        //nickname por default es vacio, entonces este endpoint permite asociar a un usuario un nickname
        //usamos JSON, un payload correcto que viaje en el body seria como el siguiente
        /*{
        "name" : 1,
        "nickname" : "Pepe"
        }*/
        //Si lo encuentra, retorno OK200, si no, retorna 404NOTFOUND.
        //https://localhost:44328/users/
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserUpdate user)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.UpdateUserAsync(user);
            if (reply.Result == true)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
            return Ok(reply);
        }

    }


}
