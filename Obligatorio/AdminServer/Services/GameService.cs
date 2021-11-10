using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminServer.Services
{
    public class GameService
    {
        internal async Task<string> GetGame(string name)
        {
            var ret = Task.Run(async () =>
            {
                string dummy = await Dummy();
                return dummy;
            });
            return ret.Result;
        }

        private async Task<string> Dummy()
        {
            return "hola";
        }
    }


}
