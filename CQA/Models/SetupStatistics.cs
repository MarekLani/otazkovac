using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    /// <summary>
    /// SetupStatistics are periodically saved to show us progress of evaluation
    /// and ratio between count of evaluations and answers
    /// </summary>
    public class SetupsStatistics
    {
        public int SetupsStatisticsId { get; set; }

        public int SetupId { get; set; }
        public virtual Setup Setup { get; set; }

        public int AnswersCount { get; set; }
        public int EvaluatedAnswersForUsersCount { get; set; }
        public int FullyEvaluatedAnswersCount { get; set; }

        /// <summary>
        /// Save statistics for each active setup such as answer counts evaluation, count evaluated answers
        /// </summary>
        //public static void SaveSetupStats()
        //{
        //    CQADBContext db = new CQADBContext();
        //    foreach (Setup s in db.Setups.Where(s => s.Active))
        //    {
        //        var setupStats = new SetupsStatistics();
        //        //setupStats.EvaluatedAnswersForUsersCount = 
                
        //        setupStats.FullyEvaluatedAnswersCount =  
        //            (from q in s.Questions
        //             from a in q.Answers
        //                .Where(ans => ans.Evaluations.Count == MyConsts.FullEvaluationLimit)
        //             select(a.AnswerId)).Count();

        //        setupStats.EvaluatedAnswersForUsersCount =
        //            (from q in s.Questions
        //             from a in q.Answers
        //                .Where(ans => ans.Evaluations.Count >= MyConsts.MinEvaluationLimit)
        //             select (a.AnswerId)).Count();

        //        setupStats.AnswersCount =
        //            (from q in s.Questions
        //             from a in q.Answers
        //             select (a.AnswerId)).Count();

        //        setupStats.Setup = s;
        //        db.SetupsStatistics.Add(setupStats);
        //        db.SaveChanges();

        //    }
        //}
    }
}