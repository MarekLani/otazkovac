using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class Setup : DateCreatedModel //: INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        //public Setup()
        //{
        //    this.PropertyChanged += new PropertyChangedEventHandler(CreateSetupsProbabilityChange);
        //}

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
        
        private int _answeringProbability = 0;
        [DisplayName("Počet hodnotení na jednu odpoveď")]
        [Required]
        [Range(1, 50)]
        public int AnsweringProbability 
        { 
            get { return _answeringProbability; }
            set {
                _answeringProbability = value;
                //OnPropertyChanged("AnsweringProbability"); 
            } 
        }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<UsersSetup> UsersSetups { get; set; }
        //not needed right now
        //public virtual ICollection<SetupsStatistics> SetupsStatistics { get; set; }
        public virtual ICollection<SetupsProbabilityChange> SetupsProbabilityChanges { get; set; }


        // Create the OnPropertyChanged method to raise the event 
        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

    }
}