using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class Notification
    {

        public int NotificationId { get; set; }

        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        public int AnswerId { get; set; }
        public virtual Answer Answer {get; set;}

        public NotificationFor NotificationFor { get; set; }

        public NotificationType NotificationType { get; set; }
    }

    public enum NotificationFor
    {
        MyAnswer,
        MyEvaluation
    }

    public enum NotificationType
    {
        NewEvaluation,
        NewComment
    }
}