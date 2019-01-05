using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{

    public class UserAvatar
    {
        public string UserId { get; set; }
        public string Gender { get; set; }
        public byte[] Avatar { get; set; }
        public string Extension { get; set; }
        public string MimeType { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsDefault { get; set; }
    }

}
