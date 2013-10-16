using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class SetupsProbabilityChange :DateCreatedModel
    {
        public int SetupsProbabilityChangeId { get; set; }

        public int SetupId { get; set; }
        public virtual Setup Setup { get; set; }

        public int Value { get; set; }
    }
}