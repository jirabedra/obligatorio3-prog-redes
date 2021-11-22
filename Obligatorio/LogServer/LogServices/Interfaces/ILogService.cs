using LogsLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogServer.LogServices.Interfaces
{
    public interface ILogService
    {
        public List<Log> FilterByGame(List<string> gameNames);
        public List<Log> FilterByUser(List<int> userIds);
        public List<Log> FilterByDate(DateTime date);
    }
}
