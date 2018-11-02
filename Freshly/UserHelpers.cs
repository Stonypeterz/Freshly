using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net.Mail;
using System.Net.Mime;
using Freshly.Identity.Models;
using Freshly.Identity.DAL;

namespace Freshly.Identity
{

    public class UserHelpers : IDisposable 
    {
        private ApplicationUsersFactory UsrDF = new ApplicationUsersFactory();
        
        #region "Protected Methods"

        protected ResultStatus ValidatePassword(string password)
        {
            //Check password strength
            if (Freshly.D.CheckPasswordStrength == true)
            {
                if (password.Length < Freshly.D.PasswordOptions.MinPasswordLength) return new ResultStatus($"The password length must be greater than or equal to {Freshly.D.PasswordOptions.MinPasswordLength}");
                if (Freshly.D.PasswordOptions.RequiredUpperCase > 0 && !Regex.IsMatch(password, $"^(?=(.*[A-Z]){"{"}{Freshly.D.PasswordOptions.RequiredUpperCase},{"}"}).+$")) return new ResultStatus($"The password must contain a minimum of {Freshly.D.PasswordOptions.RequiredUpperCase} upper cased characters");
                if (Freshly.D.PasswordOptions.RequiredLowerCase > 0 && !Regex.IsMatch(password, $"^(?=(.*[a-z]){"{"}{Freshly.D.PasswordOptions.RequiredLowerCase},{"}"}).+$")) return new ResultStatus($"The password must contain a minimum of {Freshly.D.PasswordOptions.RequiredLowerCase} lower cased characters");
                if (Freshly.D.PasswordOptions.RequiredNumericDigits > 0 && !Regex.IsMatch(password, $"^(?=(.*\\d){"{"}{Freshly.D.PasswordOptions.RequiredNumericDigits},{"}"}).+$")) return new ResultStatus($"The password must contain a minimum of {Freshly.D.PasswordOptions.RequiredNumericDigits} numeric digits");
                if (Freshly.D.PasswordOptions.RequiredNoneAlphaNumeric > 0 && !Regex.IsMatch(password, $"^(?=(.*\\W){"{"}{Freshly.D.PasswordOptions.RequiredNoneAlphaNumeric},{"}"}).+$")) return new ResultStatus($"The password must contain a minimum of {Freshly.D.PasswordOptions.RequiredNoneAlphaNumeric} none alphanumeric characters");
            }

            //If it gets here, everything went well
            return new ResultStatus(true, "Successful");
        }

        protected (Status lr, ApplicationUser usr) ValidateUser(string UserId)
        {
            var usr = UsrDF.GetUserByID(UserId);
            var lgnr = Status.Success;
            if (usr == null) lgnr = Status.InvalidAccountId;
            else if (usr.CurrentStatus == AccountStatus.Pending) lgnr = Status.Pending;
            else if (usr.CurrentStatus == AccountStatus.Active) lgnr = Status.Success;
            else if (usr.CurrentStatus == AccountStatus.Locked) {
                var lkd = AllConsts.FPA.Find(f => f.UserId == usr.UserId);
                if(lkd == null || lkd?.DateLocked.AddMinutes(Freshly.D.LockedWindow) < DateTime.UtcNow)
                {
                    usr.CurrentStatus = AccountStatus.Active;
                    UsrDF.Update(usr);
                    lgnr = Status.Success;
                    AllConsts.FPA.Remove(lkd);
                }
                else lgnr = Status.Locked;
            }
            return (lgnr, usr);
        }

        protected bool ComparePasswords(ApplicationUser usr, string Password)
        {
            var cypher = Password.ToHashed();
            //var x = usr.Password.Split('+');
            return usr.Password == cypher;
        }

        #endregion

        internal ResultStatus CreateUser(ApplicationUser usr, string password)
        {
            if (usr == null || string.IsNullOrEmpty(usr?.UserId)) return new ResultStatus("The user object is null.");
            var res = ValidatePassword(password);
            if (res.IsSuccessful)
            {
                //Check for duplicate user account info
                var extUsr = UsrDF.GetUserByID(usr.UserId);
                if (extUsr == null)
                {
                    //Hash the password
                    usr.Password = password.ToHashed();
                    //Create the account
                    UsrDF.Create(usr);
                    return new ResultStatus(true, "Successful");
                }
                else if (extUsr.UserId.ToLower() == usr.UserId.ToLower()) return new ResultStatus("The [User Id] provided is already in use by someone else.");
                else if (extUsr.Email.ToLower() == usr.Email.ToLower()) return new ResultStatus("The [Email address] provided is already in use by someone else.");
                else if (extUsr.PhoneNumber.ToLower() == usr.PhoneNumber.ToLower()) return new ResultStatus("The [Phone Number] provided is already in use by someone else.");
            }
            return res;
        }

        internal string GetMessage(Status code)
        {
            var resp = code.ToString();
            switch (code)
            {
                case Status.InvalidAccountId:
                    resp = "The Account Id you entered is wrong.";
                    break;
                case Status.Locked:
                    resp = "The account is currently locked. Please contact our support team for immediate unlocking.";
                    break;
                case Status.Failed:
                    resp = "The process failed! Please try again later.";
                    break;
                case Status.Pending:
                    resp = "Your account has not been activated yet. You cannot do anything that requires this account until it's activated.";
                    break;
                case Status.Denied:
                    resp = "You do not have the required rights to view the resources you are trying to load. Please contact support for more information.";
                    break;
                case Status.WrongPassword:
                    resp = "The password you entered is wrong.";
                    break;
                case Status.InvalidCodeOrId:
                    resp = "Invalid User Id or expired Security Code.";
                    break;
                case Status.InvalidNewPassword:
                    resp = "The new password you entered does not have all our valid password requirements.";
                    break;
            }
            return resp;
        }

        public void Dispose()
        {
            UsrDF.Dispose();
        }

    }

}
