using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class Subject
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int SubjectId { get; set; }

        [Required]
        [DisplayName("Meno predmetu")]
        public string Name { get; set; }

        [Required]
        [DisplayName("Skrátené meno")]
        public string Shortcut { get; set; }


        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Setup> Setups { get; set; }
        public virtual ICollection<Concept> Concepts { get; set; }
    }
}