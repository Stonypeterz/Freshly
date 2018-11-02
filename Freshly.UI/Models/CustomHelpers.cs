using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Net.Mail;
using System.Net.Mime;

namespace Freshly.UI.Models
{
    public class CustomHelpers : ICustomHelpers
    {
        private readonly IHttpContextAccessor context;
        public GlobalVariables GVs { get; }

        public CustomHelpers(IHttpContextAccessor ctxAccessor, IOptions<GlobalVariables> gv)
        {
            context = ctxAccessor;
            GVs = gv.Value;
        }

        public string GetMessage(MsgTypes type)
        {
            switch (type)
            {
                case (MsgTypes.GenericError):
                    return "We just encountered an error while processing your request. We've notified the support team about this. Please try again later.";
                default:
                    return type.ToString();
            }
        }

        public string GetErrorMailMessage(Exception e)
        {
            return "Dear Sir/Madam<br /><br />" +
            "We just encountered an error that you should look at.<br /><br />" +
            "<p style=\"color:red;padding:10px;\">" +
            "    <b>" + e.GetType().FullName + "</b>: " + e.Message + ",<br /><b>Source</b>: " + e.Source + " - " + context.HttpContext.Request.GetEncodedUrl() + ",<br />" +
            "    <b>User Name</b> :  " + (context.HttpContext.User.Identity.IsAuthenticated ? context.HttpContext.User.Identity.Name : "Guest") + ",<br />" +
            "    <b>User Host Address</b>: " + context.HttpContext.Connection.RemoteIpAddress +
            "</p><br /><br />" +
            "Please <a href=\"" + e.HelpLink + "\">click here</a> for help. See also the stacktrace as shown below.<br /><br />" +
            "<p style=\"background-color:yellow;padding:10px;margin:10px\">" +
                e.StackTrace +
            "</p><br /><br />" +
            "Thanks.";
        }

        public string GetAlert(Alerts MsgType, string Msg = "")
        {
            return "<div Class=\"alert alert-" + MsgType.ToString() + "\"><span data-dismiss=\"alert\" class=\"close\">&times;</span>" + Msg + "</div>";
        }

        public bool IsValid(string data, dataType type)
        {
            bool isValisd = false;
            switch (type)
            {
                case (dataType.Email):
                    {
                        isValisd = Regex.IsMatch(data.Trim(), "^\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*$");
                        break;
                    }
            }
            return isValisd;
        }

        public string LogError(Exception ex)
        {
            //
            var ml = new List<string>() { "support@stonypeterz.com" };
            SendEmail(GVs.SupportMail, $"Error {ex.TargetSite.Name}", GetErrorMailMessage(ex), ml);
            return GetMessage(MsgTypes.GenericError);
        }
        
        public string SendEmail(string to, string subject, string body, List<string> c_copies = null, List<eAttachment> attachedFile = null, string replyTo = "")
        {
            StringBuilder sp = new StringBuilder();
            try
            {
                bool isValid = false;
                MailMessage DMail = new MailMessage();
                SmtpClient MySMTP = new SmtpClient(GVs.MailServer);
                DMail.From = new MailAddress(GVs.SenderAddress, GVs.SenderName);
                to = to.Replace(",", ";");
                string[] X = to.Split(';');
                string em = "";
                for (int i = 0; i < X.Length; i++)
                {
                    em = X[i].Trim();
                    if (this.IsValid(em, dataType.Email))
                    {
                        DMail.To.Add(em);
                        isValid = true;
                    }
                }
                if (c_copies != null)
                {
                    foreach (string cc in c_copies)
                    {
                        if (this.IsValid(cc, dataType.Email)) DMail.CC.Add(cc);
                    }
                }
                DMail.Subject = subject;
                if (replyTo != null && this.IsValid(replyTo.Trim(), dataType.Email)) DMail.ReplyToList.Add(new MailAddress(replyTo.Trim()));
                else DMail.ReplyToList.Add(new MailAddress(GVs.FeedBackMail));
                if (attachedFile != null)
                {
                    int j = attachedFile.Count;
                    for (int i = 0; i < j; i++)
                    {
                        //MemoryStream dFile = new MemoryStream(AttachedFile);
                        DMail.Attachments.Add(new Attachment(attachedFile[i].fileStream, attachedFile[i].Name));
                    }

                }
                DMail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html));
                DMail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.Replace("<br />", Environment.NewLine).Replace("<", "[").Replace(">", "]"), null, MediaTypeNames.Text.Plain));
                if (isValid)
                {
                    MySMTP.Port = Convert.ToInt32(GVs.smtpPort);
                    if (GVs.smtpPort == 587) MySMTP.EnableSsl = true;
                    if ((GVs.MailServer).ToLower() != "localhost") MySMTP.Credentials = new NetworkCredential(GVs.SenderAddress, GVs.SenderPassword);
                    MySMTP.DeliveryMethod = SmtpDeliveryMethod.Network;
                    MySMTP.Send(DMail);
                }
                if (!isValid) sp.Append("The email address (" + to + ") is not valid.");
                else sp.Append("Sent successfully!");
            }
            catch (Exception ex)
            {
                sp.Append(ex.Message);
            }
            return sp.ToString();
        }

        public string GetHash(string data)
        {
            byte[] salt = new byte[16] { 0x59, 0x76, 0x68, 0x6e, 0x20, 0xfd, 0x65, 0x34, 0x70, 0x55, 0x64, 0x69, 0x77, 0xef, 0xc7, 0xff };
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: data,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 15000,
                numBytesRequested: 32));
            return hashed;
        }

        //store your key is a hidden place (Not like this)
        private string eKey = "J85a/gPN5mWh/jMQJPK!EWiMN5B@zWLU4TZn0fUSzJPRaw";
        private byte[] bt = new byte[16] { 0x99, 0xf6, 0x68, 0x6e, 0x50, 0xfd, 0x69, 0x64, 0x7a, 0x55, 0x64, 0x69, 0x77, 0xbd, 0xe6, 0xa9 };

        public string Encrypt(string clearText, string ownerdata)
        {
            string EncryptionKey = ownerdata.ToUpper() + eKey;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bt, 15000);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public string Decrypt(string cipherText, string ownerdata)
        {
            string EncryptionKey = ownerdata.ToUpper().Trim() + eKey;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bt, 15000);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

    }

    public interface ICustomHelpers
    {
        GlobalVariables GVs { get; }

        string GetMessage(MsgTypes type);
        string GetAlert(Alerts type, string Msg = "");
        string SendEmail(string to, string subject, string body, List<string> c_copies = null, List<eAttachment> attachedFile = null, string replyTo = "");
        string LogError(Exception ex);
        string GetErrorMailMessage(Exception e);
        bool IsValid(string data, dataType type);
        string GetHash(string data);
        string Encrypt(string clearText, string ownerdata);
        string Decrypt(string cipherText, string ownerdata);
    }

    public class eAttachment
    {
        public string Name { get; set; }
        public MemoryStream fileStream { get; set; }
    }

    public class GlobalVariables
    {
        public string CompanyName { get; set; }
        public string ServerUrl { get; set; }
        public string MailServer { get; set; }
        public string SenderName { get; set; }
        public string SenderAddress { get; set; }
        public string SenderPassword { get; set; }
        public int smtpPort { get; set; }
        public string SupportMail { get; set; }
        public string FeedBackMail { get; set; }
        public string Tovapaky { get; set; }
        public int PageSize { get; set; }

    }
}

public static class CustomHelperExtension
{
    public static string Encode(this IUrlHelper Url, string data)
    {
        return WebUtility.UrlEncode(data);
    }

    public static byte[] EncodeToBytes(this IUrlHelper Url, byte[] value, int offset, int count)
    {
        return WebUtility.UrlEncodeToBytes(value, offset, count);
    }

    public static string Decode(this IUrlHelper Url, string data)
    {
        return WebUtility.UrlDecode(data);
    }

    public static byte[] DecodeToBytes(this IUrlHelper Url, byte[] encodedValue, int offset, int count)
    {
        return WebUtility.UrlDecodeToBytes(encodedValue, offset, count);
    }

    public static bool IsAjaxRequest(this HttpRequest request)
    {
        if (request.Headers == null) return true;
        if (request.Headers["X-Requested-With"] == "XMLHttpRequest") return true;
        return false;
    }
    
    public static string AbsoluteUri(this HttpRequest request)
    {
        return request.GetEncodedUrl().Replace("?r=1", "").Replace("&r=1", "");
    }

    public static string RootUrl(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host.ToUriComponent()}/";
    }
}

public enum Alerts
{
    success,
    warning,
    danger,
    info
}

public enum MsgTypes
{
    Success,
    AccessDenied,
    LoginFailed,
    AccountLocked,
    GenericError
}

public enum dataType
{
    Email
}

public static class PTNames
{
    public const string Groups = "Groups";
    public const string Edit = "Edit";
}
