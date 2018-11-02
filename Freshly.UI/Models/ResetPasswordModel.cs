using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.UI.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string Password { get; set; }
        [Compare(nameof(Password), ErrorMessage = "This value is not the same as that of the Password field")]
        public string ConfirmPassword { get; set; }
    }
}
