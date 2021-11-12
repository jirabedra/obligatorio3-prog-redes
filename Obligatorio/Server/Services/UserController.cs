using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services
{
    public class UserController : UserService.UserServiceBase
    {

        public UserController(){

        }
        public override Task<Response> AddUser(UserProto request, ServerCallContext context)
        {
            Console.WriteLine("HOLA");
            throw new Exception();
            return null;
        }

    }
}
