using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    public enum Status
    {
        Active,
        Failed,
        InvalidAccountId,
        InvalidNewPassword,
        Locked,
        Pending,
        Success,
        WrongPassword,
        InvalidCodeOrId,
        Denied,
        Suspended
    }

    public static class AccountStatus
    {
        public const string Pending = "Pending";
        public const string Active = "Active";
        public const string Locked = "Locked";
        public const string Suspended = "Suspended";
    }

    public class ResultStatus
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }

        public ResultStatus() : this(false, "No record was affected.") { }

        public ResultStatus(string message):this(false, message) { }
        
        public ResultStatus(bool success, string message)
        {
            IsSuccessful = success;
            Message = message;
        }
    }
}
