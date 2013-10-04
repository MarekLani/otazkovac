using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class UsersSetupAction 
    {
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        public int SetupId { get; set; }
        public virtual Setup Setup { get; set; }

        public int Action { get; set; }

    }
}
