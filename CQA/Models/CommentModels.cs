using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CQA.Resources;

namespace CQA.Models
{
    public class Comment : DateCreatedModel
    {
        public int CommentId { get; set; }

        public int UserId { get; set; }
        public virtual UserProfile Author { get; set; }

        public int AnswerId { get; set; }
        public virtual Answer Answer { get; set; }

        [DisplayName("Váš komentár")]
        [Required(ErrorMessage = "Prosím zadajte text komentára")]
        public string Text { get; set; }

        [DisplayName("Uložiť ako anonymný komentár")]
        public bool Anonymous { get; set; }

        public Comment()
        {
        }

        public Comment(int AnswerId)
        {
            this.AnswerId = AnswerId;
        }

    }

    public class ViewComment
    {
        public string UserName {get; set;}
        public string Text { get; set; }
        public int AnswerId { get; set; }

        public ViewComment(string text, string userName, int answerId)
        {
            this.UserName = userName;
            this.Text = text;
            this.AnswerId = answerId;
        }
    }
}