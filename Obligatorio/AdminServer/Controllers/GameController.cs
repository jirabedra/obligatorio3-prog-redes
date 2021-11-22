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
    [Route("games")]
    [ApiController]
    public class GameController : Controller
    {


        //http://localhost:5561/games/ POST
        /*
         *  {
           "nickname": "Zelda",
            "genre": "action"
            }
         */
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GameProto game)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.AddGameAsync(
                              game); ;
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
        public async Task<IActionResult> Delete(string name)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.DeleteGameAsync(
                new GameName()
                {
                    Name = name
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
        //https://localhost:44328/games/
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] GameUpdate game)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new UserService.UserServiceClient(channel);
            var reply = await client.UpdateGameAsync(game);
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
