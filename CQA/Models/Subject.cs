using System;
using System.Collections.Generic;
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
        public string Name { get; set; }

        [Required]
        public string Shortcut { get; set; }

        public virtual ICollection<Setup> Setups { get; set; }
    }
}