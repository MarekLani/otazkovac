using System;
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

        [DisplayName("Pomer hodnotení k odpovediam (uvedťe počet hodnotení na jednu odpoveď)")]
        [Required]
        [Range(1,50)]
        public int AnsweringProbability { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual ICollection<UsersSetup> UsersSetups { get; set; }

        public virtual ICollection<SetupStatistics> SetupStatistics { get; set; }

    }
}