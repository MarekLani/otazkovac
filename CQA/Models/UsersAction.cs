using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class UsersAction : DateCreatedModel
    {
        [Key]
        public int UsersActionId { get; set; }

        public int UserId { get; set; }
        public virtual UserProfile User { get; set; }

        public int? AnswerId { get; set; }
        public virtual Answer Answer { get; set; }

        public int? QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public virtual UserActionType Action { get; set; }

    }


    public enum UserActionType
    {
        // !IMPORTANT whe adding new UserActionType, add it always to the end!!!
        Evaluation = 1,
        Answering,
        SkippedAnswering,
        SkippedEvaluation,
        ViewedHintWhenAnswering,
        ViewedHintWhenEvaluating,
        Commented
        
    }
}
