using LogServer.LogHandling;
using LogServer.LogServices.Interfaces;
using LogsLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogServer.LogServices.Implementations
{
    public class LogService : ILogService
    {
        private static readonly LogHandler _logHandler = new LogHandler();
        public static List<Log> _logs { get; private set; }
        private Semaphore logsSemaphore = new Semaphore(1, 1);

        public void UpdateLogs()
        {
            List<string> newLogsAsStrings = _logHandler.UpdateLogs();
            List<Log> newLogs = ProcesssNewLogs(newLogsAsStrings);
            logsSemaphore.WaitOne();
            _logs.AddRange(newLogs);
            logsSemaphore.Release();
        }

        private List<Log> ProcesssNewLogs(List<string> newLogsAsStrings)
        {
            throw new NotImplementedException();
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
