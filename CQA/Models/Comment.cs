using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CQA.Resources;

namespace CQA.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        public double Text { get; set; }

        public int UserId { get; set; }
        public virtual UserProfile Author { get; set; }

        public int AnswerId { get; set; }
        public virtual Answer Answer { get; set; } 
    }
}