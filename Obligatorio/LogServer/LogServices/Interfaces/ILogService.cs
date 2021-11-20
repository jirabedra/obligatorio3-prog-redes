using LogsLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogServer.LogServices.Interfaces
{
    public interface ILogService
    {
        public List<Log> FilterByGame(string gameName);
        public List<Log> FilterByUser(string userNickName);
        public List<Log> FilterByDate(DateTime date);
    }
}
