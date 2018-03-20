//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Projekt.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Comment
    {
        public int comment_id { get; set; }
        public int post_id { get; set; }
        public int user_id { get; set; }

        [Required(ErrorMessage = "You must enter a comment"), StringLength(50, MinimumLength = 1, ErrorMessage = "Your comment must be between 1 and 50 characters.")]
        public string comment { get; set; }
        public System.DateTime created { get; set; }
    
        public virtual Post Post { get; set; }
        public virtual User User { get; set; }
    }
}
