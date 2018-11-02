using Freshly.Identity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.UI.Models
{
    public class SignUpModel
    {
        [Required()]
        [MaxLength(128, ErrorMessage = "Maximum number of characters allowed is 128")]
        public string UserId { get; set; }

        [Required()]
        [MaxLength(256, ErrorMessage = "Maximum number of characters allowed is 256")]
        public string FirstName { get; set; }

        [Required()]
        [MaxLength(256, ErrorMessage = "Maximum number of characters allowed is 256")]
        public string LastName { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? DateOfBirth { get; set; }

        [Required()]
        [MaxLength(128, ErrorMessage = "Maximum number of characters allowed is 128")]
        public string Gender { get; set; }

        [Required()]
        [EmailAddress()]
        [MaxLength(256, ErrorMessage = "Maximum number of characters allowed is 256")]
        public string Email { get; set; }

        
        [MaxLength(128, ErrorMessage = "Maximum number of characters allowed is 128")]
        public string PhoneNumber { get; set; }
        
    }
}
