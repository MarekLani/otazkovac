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
    public class Answer : DateCreatedModel
    {
        public int AnswerId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        public string Text { get; set; }

        public double ExpertRating { get; set; }

        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }

        public int UserId { get; set; }
        public virtual UserProfile Author{ get; set; }

        /// <summary>
        /// Flag for determining if Answer have enough evaluations to be displayed to author
        /// NULL = not enough ratings
        /// False = enough ratings but not seen
        /// True = already seen
        /// </summary>
        public bool? SeenEvaluation { get; set; }

        public virtual ICollection<Evaluation> Evaluations { get; set; }
        public virtual ICollection<UsersAction> UsersActions { get; set; }


        public double GetAvgEvaluation()
        {
            double total = 0;
            foreach (var e in this.Evaluations)
            {
                total += e.Value;
            }
            return total / this.Evaluations.Count();
        }

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

        private string _text = "";
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [DisplayName("Vaša odpoveď:")]
        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        public string Text
        {
            get { return HttpUtility.HtmlEncode(_text); }
            set { _text = value; }
        }

    }

    public class EvaluatedAnswers
    {
        public Setup Setup { get; set; }
        public ICollection<Answer> Answers { get; set; }
        public ICollection<Answer> UnseenHighlightedAnswers { get; set; }
        public int UnseenCount {get;set;}
    }
}