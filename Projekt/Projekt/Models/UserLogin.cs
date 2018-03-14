using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Projekt.Models
{
    public class UserLogin : User
    {

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; }
    }
}