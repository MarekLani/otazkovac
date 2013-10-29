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

        public static void CreateSetupsProbabilityChange(int setupId, int value)
        {
            SetupsProbabilityChange spc = new SetupsProbabilityChange();
            spc.SetupId = setupId;
            spc.Value = value;
            CQADBContext db = new CQADBContext();
            db.SetupsProbabilityChanges.Add(spc);
            db.SaveChanges();
        }
    }
}