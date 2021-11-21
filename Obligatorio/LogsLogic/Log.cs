using Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogsLogic
{
    public class Log
    {
        public OperationType OperationType { get; set; }
        public int UserId { get; set; }
        public string GameTitle { get; set; }
        public DateTime Date { get; set; }
        public bool Result { get; set; }
        public string UserNewNickName { get; set; }
    }
}
