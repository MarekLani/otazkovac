using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class Error : DateCreatedModel
    {
        public int ErrorId { get; set; }

        public virtual UserActionType Action { get; set; }

        public string Data{ get; set; }

        public string ExceptionMessage { get; set; }

        public Error(UserActionType action, string data, string exceptionMessage)
        {
            this.Data = data;
            this.ExceptionMessage = exceptionMessage;
            this.Action = action;
        }
    }
}