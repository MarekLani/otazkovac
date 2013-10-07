﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DisplayName("Názov (rok)")]
        public string Name { get; set; }

        [DisplayName("Aktívny")]
        public bool Active { get; set; }

        [DisplayName("Meno predmetu")]
        public int SubjectId { get; set; }
        [DisplayName("Meno predmetu")]
        public virtual Subject Subject { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<UsersSetup> UsersSetups { get; set; }
    }
}