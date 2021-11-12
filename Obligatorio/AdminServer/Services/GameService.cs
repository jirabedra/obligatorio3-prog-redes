using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminServer.Services
{
    public class GameService
    {
        private UserService.UserServiceClient _client;
        internal async Task<string> GetGame(string name)
        {
            var ret = Task.Run(async () =>
            {
                string dummy = await Dummy();
                return dummy;
            });
            Channel channel = new Channel("localhost", 5010, ChannelCredentials.Insecure);
            _client = new UserService.UserServiceClient(channel);
            UserProto userProto = new UserProto() { };
            _client.AddUser(userProto);
            
            return ret.Result;
        }

        private async Task<string> Dummy()
        {
            return "hola";
        }
    }


}
