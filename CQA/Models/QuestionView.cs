using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class QuestionView
    {

        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public DateTime ViewDate { get; set; }

    }
}