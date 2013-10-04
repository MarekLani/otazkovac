using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class Setup
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int SetupId { get; set; }

        [Required]
        public string Name { get; set; }

        public bool Active { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<UsersSetup> UsersSetups { get; set; }
        public virtual ICollection<UsersSetupAction> UsersSetupActions { get; set; }
    }
}