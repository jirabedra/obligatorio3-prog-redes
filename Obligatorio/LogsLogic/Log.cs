using System;
using System.Collections.Generic;
using System.Text;

namespace LogsLogic
{
    public class Log
    {
        public string OperationType { get; set; }
        public string IntendedOperation { get; set; }
        public DateTime Date { get; set; }
        public bool Result { get; set; }
    }
}
