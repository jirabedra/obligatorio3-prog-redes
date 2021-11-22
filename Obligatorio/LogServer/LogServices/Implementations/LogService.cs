using Logging;
using LogServer.LogHandling;
using LogServer.LogServices.Interfaces;
using LogsLogic;
using Newtonsoft.Json;
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
            List<Log> newLogs = new List<Log>();
            foreach (var log in newLogsAsStrings)
            {
                Log newLog = JsonConvert.DeserializeObject<Log>(log);
                newLogs.Add(newLog);
            }
            return newLogs;
        }

        public List<Log> FilterByDate(DateTime date)
        {
            UpdateLogs();
            logsSemaphore.WaitOne();
            List<Log> auxLog = new List<Log>(_logs);
            logsSemaphore.Release();

            throw new NotImplementedException();

        }

        public List<Log> FilterByGame(List<string> gameNames)
        {
            UpdateLogs();
            logsSemaphore.WaitOne();
            List<Log> auxLog = new List<Log>(_logs);
            logsSemaphore.Release();
            List<Log> ret = new List<Log>();
            bool validOperationType;
            foreach (var log in auxLog)
            {
                validOperationType = log.OperationType == OperationType.AGame || log.OperationType == OperationType.BGame || log.OperationType == OperationType.MGame || log.OperationType == OperationType.AsocGameUser;
                if (validOperationType)
                {
                    if (gameNames.Contains(log.GameTitle))
                    {
                        ret.Add(log);
                    }
                }
            }
            return ret;
        }

        public List<Log> FilterByUser(List<int> userIds)
        {
            UpdateLogs();
            logsSemaphore.WaitOne();
            List<Log> auxLog = new List<Log>(_logs);
            logsSemaphore.Release();
            List<Log> ret = new List<Log>();
            bool intendedOperationType;
            foreach (var log in auxLog)
            {
                intendedOperationType = log.OperationType == OperationType.AUser || log.OperationType == OperationType.BUser || log.OperationType == OperationType.MUser || log.OperationType == OperationType.AsocGameUser;
                if (intendedOperationType)
                {
                    if (userIds.Contains(log.UserId))
                    {
                        ret.Add(log);
                    }
                }
            }
            return ret;
        }

        public List<Log> GetAllLogs()
        {
            UpdateLogs();
            logsSemaphore.WaitOne();
            List<Log> auxLog = new List<Log>(_logs);
            logsSemaphore.Release();
            return auxLog;
        }
    }
}
