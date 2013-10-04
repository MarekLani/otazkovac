using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CQA.Resources;

namespace CQA.Models
{
    public class Evaluation
    {

        public int UserId { get; set; }
        public virtual UserProfile Author { get; set; }

        public int AnswerId { get; set; }
        public virtual Answer Answer { get; set; } 

        [Required(ErrorMessageResourceType = typeof(ErrorStrings), ErrorMessageResourceName = "Required")]
        public double Value { get; set; }
       
    }
}