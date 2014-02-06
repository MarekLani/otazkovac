using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using CQA.Resources;

namespace CQA.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        public int QuestionFileId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        [DataType(DataType.MultilineText)]
        public string QuestionText { get; set; }

        public string Hint { get; set; }

        public string ImageUri { get; set; }

        public bool IsActive { get; set; }

        public int SetupId { get; set; }
        public virtual Setup Setup { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
        public virtual ICollection<UsersAction> UsersActions { get; set; }
        public virtual ICollection<QuestionView> QuestionViews { get; set; }
        public virtual ICollection<Concept> Concepts { get; set; }

    }
}