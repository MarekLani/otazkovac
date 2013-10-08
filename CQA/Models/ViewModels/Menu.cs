using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class Menu
    {
        public List<Setup> Setups { get; set; }
        public bool SubjectsNotEmpty { get; set; }
        public int UnseenEvaluatedAnswersCount { get; set; }
    }
}