using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    internal class AttemptModel
    {
        public string UserId { get; set; }
        public int FaildAttempts { get; set; }
        public DateTime DateLocked { get; set; }

        public AttemptModel(string userId)
        {
            UserId = userId;
            FaildAttempts = 1;
            DateLocked = DateTime.UtcNow;
        }
    }
}
