using Freshly.Identity.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity
{
    internal static class AllConsts
    {
        public const string AuthenticationType = "Freshly";
        internal static List<CodeModel> RPR = new List<CodeModel>();
        /// <summary>
        /// Failed Password Attempts
        /// </summary>
        internal static List<AttemptModel> FPA = new List<AttemptModel>();
    }
}
