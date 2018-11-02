using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    public class PasswordOptions
    {
        public int MinPasswordLength { get; set; } = 8;
        public int RequiredUpperCase { get; set; } = 1;
        public int RequiredLowerCase { get; set; } = 1;
        public int RequiredNumericDigits { get; set; } = 1;
        public int RequiredNoneAlphaNumeric { get; set; } = 1;
    }
}
