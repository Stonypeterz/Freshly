using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    internal class CodeModel
    {
        public string UserId { get; set; }
        public string Salt { get; set; }
        public string Code { get; set; }
        public DateTime Expires { get; set; }

        public CodeModel(ApplicationUser usr) {
            GenerateCode(usr);
        }

        public CodeModel(string userid)
        {
            ApplicationUser usr = null;
            using(var df = new ApplicationUsersFactory())
            {
                usr = df.GetUser(userid);
            }
            
            if (usr?.CurrentStatus == AccountStatus.Active) { GenerateCode(usr); }
            else { throw new Exception($"This ({userid}) is not a valid User ID."); }
        }

        private void GenerateCode(ApplicationUser usr)
        {
            UserId = usr.UserId;
            Salt = $"{Guid.NewGuid().ToString()}{Guid.NewGuid().ToString()}";
            Code = $"{Salt}{UserId}".ToHashed();
            Expires = DateTime.UtcNow.AddHours(3);
        }
    }
}
