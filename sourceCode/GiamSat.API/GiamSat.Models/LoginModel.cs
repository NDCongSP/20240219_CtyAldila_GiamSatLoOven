using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
