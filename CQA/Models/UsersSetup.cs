using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class UsersSetup:DateCreatedModel
    {
        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        public int SetupId { get; set; }
        public virtual Setup Setup { get; set; }
        
        public double Score { get; set; }

    }
}