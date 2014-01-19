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
        
        /// <summary>
        /// Count of unseen newly evaluated answers of user
        /// </summary>
        public int UnseenEvaluatedAnswersCount { get; set; }

        /// <summary>
        /// Count of unseen newly evaluated answers which user evaluated
        /// </summary>
        public int UnseenEvaluatedEvaluationsCount { get; set; }
    }
}