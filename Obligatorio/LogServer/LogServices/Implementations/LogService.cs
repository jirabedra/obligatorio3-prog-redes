using LogServer.LogHandling;
using LogServer.LogServices.Interfaces;
using LogsLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogServer.LogServices.Implementations
{
    public class LogService : ILogService
    {
        private static readonly LogHandler _logHandler = new LogHandler();
        public static List<Log> _logs { get; private set; }
        public LogHandler logHandler { get; set; }

        public void AddLog() 
        { 
            
        }
        public List<Log> FilterByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<Log> FilterByGame(string gameName)
        {
            throw new NotImplementedException();
        }

        public List<Log> FilterByUser(string userNickName)
        {
            throw new NotImplementedException();
        }
    }
}
