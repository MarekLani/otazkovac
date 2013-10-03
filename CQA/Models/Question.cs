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

        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        [DataType(DataType.MultilineText)]
        public string QuestionText { get; set; }

        public string Hint { get; set; }

        public string ImageUri { get; set; }

        public virtual ICollection<Answer> Answers { get; set;}

        public int SetupId { get; set; }
        public virtual Setup Setup { get; set; }

    }
}