using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Freshly.Identity.Models
{

    public class UserAvatar
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Gender { get; set; }
        public byte[] Avatar { get; set; }
        public string Extension { get; set; }
        public string MimeType { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDefault { get; set; }
    }

}
