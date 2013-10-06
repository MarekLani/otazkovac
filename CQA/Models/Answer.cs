using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CQA.Resources;

namespace CQA.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        public string Text { get; set; }

        public double ExpertRating { get; set; }

        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public int UserId { get; set; }
        public virtual UserProfile Author{ get; set; }

        public virtual ICollection<Evaluation> Evaluations { get; set; }
        public virtual ICollection<UsersAction> UsersActions { get; set; }

    }

    public class CreateAnswer
    {

        public CreateAnswer(int questionId)
        {
            this.QuestionId = questionId;
        }

        [Required]
        [HiddenInput(DisplayValue = false)]
        public int QuestionId { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("Vaša odpoveď:")]
        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        public string Text { get; set; }

    }
}