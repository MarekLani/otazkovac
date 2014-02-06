using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace CQA.Models
{
    public class Concept
    {
        public int ConceptId { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public string Value { get; set; }
        public string ActiveWeeks { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        [NotMapped]
        public virtual List<int> ActiveWeeksList
        {
            get
            {
                if (ActiveWeeks == null || ActiveWeeks == "")
                    return new List<int>();
                return new JavaScriptSerializer().Deserialize<List<int>>(ActiveWeeks);
            }
            set
            {
                ActiveWeeks = new JavaScriptSerializer().Serialize(value);
            }
        }
    }
}