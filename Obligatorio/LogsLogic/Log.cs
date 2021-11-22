using Logging;
using System;
using System.Collections.Generic;
using System.Text;



    public class Log
    {
        public OperationType OperationType { get; set; }
        public int UserId { get; set; }
        public string GameTitle { get; set; }
        public DateTime Date { get; set; }
        public bool Result { get; set; }
        public string UserNewNickName { get; set; }

    public override string ToString()
    {
        return $"OperationType: {OperationType}.\nUserId: {UserId}\nGameTitle: {GameTitle}\nDate: {Date}\nResult: {Result}\nUserNickName: {UserNewNickName}";
    }
}

