using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CQA.Models
{
    public class UploadedImage
    {

        public Guid UploadedImageId { get; set; }
        
        public string ImageUrl { get; set; }
        public int Position { get; set; }

        public int UserId { get; set; }
        // For future purposes relate it to userProfile, not only to trainer
        public virtual UserProfile Owner { get; set; } 
    }
}