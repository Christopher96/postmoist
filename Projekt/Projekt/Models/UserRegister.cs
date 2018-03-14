using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Projekt.Models
{
    public class UserRegister : UserLogin
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string confirm_password { get; set; }
    }
}