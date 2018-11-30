using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Freshly.Identity;
using Microsoft.AspNetCore.Authorization;
using Freshly.UI.Models;
using Freshly.Identity.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Freshly.UI.Controllers
{
    [Authorize(AuthenticationSchemes = authScheme)]
    public class AccountController : Controller
    {
        private const string authScheme = CookieAuthenticationDefaults.AuthenticationScheme + "," + JwtBearerDefaults.AuthenticationScheme;
        private readonly UserManager userManager;
        private readonly GroupManager groupManager;
        private readonly ICustomHelpers AppUtil;

        public AccountController(UserManager _usrMgr, GroupManager _gm, ICustomHelpers _appUtil)
        {
            userManager = _usrMgr;
            groupManager = _gm;
            AppUtil = _appUtil;
        }

        [AllowAnonymous]
        public IActionResult Signup()
        {
            return View(new SignUpModel() { Gender = "Male" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignUpModel model)
        {
            if (ModelState.IsValid) {
                var pass = $"P{Guid.NewGuid().ToString().Substring(0, 7)}$1";
                var usr = new ApplicationUser() { UserId = model.UserId, FirstName = model.FirstName, LastName = model.LastName, Gender = model.Gender, DateOfBirth = model.DateOfBirth, Email = model.Email, PhoneNumber = model.PhoneNumber };
                var res = await userManager.CreateAsync(usr, pass);
                if (res.IsSuccessful) {
                    var code = $"{model.Email}{model.UserId}".ToHashed();
                    AppUtil.SendEmail(model.Email, "Activate your account", $"Dear {model.FirstName},<br/><br/>Thank you for joining us. Your account has been provisioned and we can't wait to have you start enjoying services. You just need to activate it. <a href=\"{Request.RootUrl()}account/activate/{model.UserId}?code={code}\">Click here</a> to activate your account and join other user already enjoying our services.<br/><br/>Once again, we are happy to have you here. Thanks.");
                    return View("Resp", new ResponseObject() { Alert = Alerts.success, Title = "New Account created", Msg = $"Congratulation! Your account was created successfully. We've sent a link to your email address. Use it to activate your account. Be sure to check your junk folder if the mail is not in your inbox." });
                } else {
                    ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, res.Message);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Activate(string id, string code)
        {
            var res = new ResponseObject() { Title = "Error", Msg = "The activation link is not valid" };
            if(!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(code)) {
                var usr = await userManager.GetUserAsync(id);
                if(usr != null) {
                    var x = $"{usr.Email}{usr.UserId}".ToHashed();
                    if (x == code) {
                        var r = await userManager.ActivateAsync(usr);
                        if (r.IsSuccessful) {
                            res.Alert = Alerts.success;
                            res.Msg = $"Congratulations! Your account was activated successfully. <a href=\"{Request.RootUrl()}@{usr.UserId}\">Click here</a> to login and see your details.";
                        } else {
                            res.Msg = r.Message;
                        }
                    }
                }
            }
            return View("Resp", res);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl) => View(new LoginModel() { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var r = await userManager.SignInAsync(model.UserId, model.Password, model.RememberMe);
                if (r.IsSuccessful) return RedirectToLocal(model.ReturnUrl);
                else ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, r.Message);
            }
            //If it gets here, something went wrong
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateToken()
        {
            var token = await userManager.GenerateTokenAsync("stonypeterz", "Admin.1st", AppUtil.GVs.ServerUrl, AppUtil.GVs.ServerUrl, AppUtil.GVs.Tovapaky);
            return Ok(token);
        }

        [Route("/@{UserId}/{tab?}")]
        public async Task<IActionResult> Profile(string UserId, string tab)
        {
            if (!string.IsNullOrEmpty(UserId)) {
                UserId = UserId.Replace("@", "");
                var usr = await userManager.GetUserAsync(UserId);
                if (usr != null) {
                    if(User.Identity.IsAuthenticated && (UserId.ToLower() == User.Identity.Name.ToLower() || User.AdheresTo(PolicyTypes.ViewOtherProfiles))) {
                        ViewBag.Tab = tab;
                        return View(usr);
                    } else {
                        return View("ProfileBasics", usr);
                    }
                }
                else return View("Resp", new ResponseObject() { Title = "Invalid Account Id", Msg = $"We could not find any user account with a corresponding user id or email ({UserId})." });
            } else return View("Resp", new ResponseObject() { Msg = "A user id or email address is required." });
        }

        public async Task<IActionResult> Groups(string id)
        {
            var gs = await userManager.GetAssignedGroupsAsync(id);
            return View(gs);
        }
        
        [HttpPost()]
        public async Task<IActionResult> AddToGroups(string tab, string UserId, string[] GroupNames)
        {
            await userManager.AddUserToGroupsAsync(UserId, GroupNames);
            if (string.IsNullOrEmpty(tab)) return RedirectToAction("Groups", new { id = UserId });
            else return Redirect("/@{UserId}/groups");
        }

        [Authorize(Policy = PolicyTypes.ListUsers)]
        public async Task<IActionResult> List(string gn, int pageno, int pagesize, string q)
        {
            var lst = await userManager.GetUsersAsync(gn, pageno, pagesize, q);
            return View(lst);
        }

        public async Task<IActionResult> Logout(string ReturnUrl)
        {
            await userManager.SignOutAsync();
            return RedirectToLocal(ReturnUrl);
        }

        public IActionResult ChangePassword() => View(new ChangePasswordModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var r = await userManager.ChangePasswordAsync(User.Identity.Name, model.OldPassword, model.NewPassword);
                if (r.IsSuccessful) return RedirectToAction("Index", "Home");
                else ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, r.Message); //ModelState.AddModelError("", r.Message);
            }
            return View(model);
        }

        [AllowAnonymous]
        [ResponseCache(CacheProfileName = "Default")]
        public async Task Avatar(string id) {
            var avt = await userManager.GetAvatarAsync(id);
            var i = avt.Avatar.Length;
            Response.ContentType = avt.MimeType;
            Response.Body.Write(avt.Avatar, 0, i);
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string UserId) {
            if (string.IsNullOrEmpty(UserId))
            {
                ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, "Your Account Id (User Name or Email Address) is required.");
                return View();
            }
            var usr = await userManager.GetUserAsync(UserId);
            if(usr != null) {
                var code = await userManager.GenerateResetPasswordCodeAsync(UserId);
                var link = $"{Request.RootUrl()}account/resetpassword?code={Url.Encode(code)}";
                AppUtil.SendEmail(usr.Email, "Reset your password", $"Dear {usr.FirstName},</br></br>We got a request for password reset from your account. If you are the one that sent it, please <a href=\"{link}\">click here</a> or copy this link ({link}) to your browser to continue. Ignore this mail if you did not send such request.</br></br>Thanks.");
            }
            return View("Resp", new ResponseObject() { Alert = Alerts.success, Title = "Forgot password", Msg = "We got your request. We've also sent you a link through which you can reset your password. Please check your mail to continue." });
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string code) => View(new ResetPasswordModel() { Code = code });

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Code)) {
                ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, "Your Account Id (User Name or Email Address) is required.");
                return View();
            }
            if (ModelState.IsValid) {
                var res = await userManager.ResetPasswordAsync(model.Code, model.Password);
                if (res.IsSuccessful) {
                    return View("Resp", new ResponseObject() { Alert = Alerts.success, Title = "Reset password", Msg = "Your password reset was successful. <a href=\"/security/login\">Click here</a> to login with your new password." });
                } else {
                    ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, res.Message);
                    return View(model);
                }
            } else {
                return View(model);
            }
        }

        public IActionResult UpdateAvatar()
        {
            return View();
        }

        [HttpPost()]
        [ValidateAntiForgeryToken()]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            var msg = "";
            try {
                if (file != null) {
                    if (file.ContentType.Contains("image/")) {
                        var x = file.FileName.Split('.');
                        var obj = new UserAvatar() {
                            UserId = User.Identity.Name,
                            Gender = User.GetGender(),
                            Extension = x[x.Length - 1],
                            MimeType = file.ContentType,
                            DateUpdated = DateTime.Now 
                        };
                        using (var mStream = new MemoryStream()) {
                            await file.CopyToAsync(mStream);
                            obj.Avatar = mStream.ToArray();
                        }
                        await userManager.UpdateAvatarAsync(obj);
                        return View("Toast", new ResponseObject(Alerts.success, "Profile picture update", "Your new profile picture was uploaded successfully! However, we take our time to display new picture. Don't worry, it won't take more than an hour to start showing in your profile."));
                    } else { msg = "Sorry, we only accept image files for profile pictures. Try uploading a JPEG, GIF or PNG files"; }
                } else { msg = "No image file was uploaded."; }
            } catch (Exception ex) {
                msg = AppUtil.LogError(ex);
            }
            return View("Toast", new ResponseObject(Alerts.danger, "Profile picture update", msg));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                return View("Resp", new ResponseObject(Alerts.danger, "Required data missing", "Sorry, the required User Id or Email Address is missing in the link you clicked on."));
            }
            if (id.ToLower() != User.Identity.Name.ToLower() && id.ToLower() != User.GetEmail().ToLower() && !User.AdheresTo(PolicyTypes.EditOtherProfiles) ) {
                return View("Resp", new ResponseObject(Alerts.danger, "Access denied", "Sorry, you do not have the required rights to edit other user's profile information."));
            }
            var usr = await userManager.GetUserAsync(id);
            if(usr != null) {
                var model = new UserUpdateModel() {
                    UserId = usr.UserId,
                    FirstName = usr.FirstName,
                    LastName = usr.LastName,
                    Email = usr.Email,
                    PhoneNumber = usr.PhoneNumber
                };
                return View(model);
            } else {
                return View("Resp", new ResponseObject(Alerts.danger, "Invalid Id or Email Address", "Sorry, the user id or email address your entered is wrong."));
            }
        }

        [HttpPost()]
        [ValidateAntiForgeryToken()]
        public async Task<IActionResult> Edit(UserUpdateModel model)
        {
            if (ModelState.IsValid) {
                if(model.UserId != User.Identity.Name && !User.AdheresTo(PolicyTypes.EditOtherProfiles)) {
                    return View("Resp", new ResponseObject(Alerts.danger, "Access denied", "Sorry, you do not have the required rights to edit other user's profile information."));
                }
                var usr = await userManager.GetUserAsync(model.UserId);
                usr.FirstName = model.FirstName;
                usr.LastName = model.LastName;
                usr.Email = model.Email;
                usr.PhoneNumber = model.PhoneNumber;
                var r = await userManager.UpdateAsync(usr);
                if (r.IsSuccessful) return View("Resp", new ResponseObject(Alerts.success, "Profile updated", $"The profile information was updated successfully! <a href=\"{Request.RootUrl()}@{usr.UserId}\">Click here</a> to view the profile details."));
                else ViewBag.Msg = AppUtil.GetAlert(Alerts.danger, r.Message);
            }
            return View(model);
        }

        [Authorize(Policy = PolicyTypes.DeleteUser)]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                return View("Resp", new ResponseObject(Alerts.danger, "Required data missing", "Sorry, the required User Id or Email Address is missing in the link you clicked on."));
            }
            var usr = await userManager.GetUserAsync(id);
            if (usr != null) return View(usr);
            else return View("Resp", new ResponseObject(Alerts.danger, "Invalid Id or Email Address", "Sorry, the user id or email address your entered is wrong. <a href=\"/account/list\">Click here</a> to view users list."));
        }

        [HttpPost()]
        [ValidateAntiForgeryToken()]
        [Authorize(Policy = PolicyTypes.DeleteUser)]
        public async Task<IActionResult> Delete(string id, string fullname)
        {
            ResultStatus r = await userManager.DeleteAsync(id);
            if (r.IsSuccessful) return View("Resp",new ResponseObject(Alerts.success, "Account deleted", $"{fullname}'s was deleted successfully. <a href=\"/account/list\">Click here</a> to return to users list"));
            else return View("Resp", new ResponseObject(Alerts.danger, "Delete Account", r.Message));
        }

        #region Helpers

        public IActionResult RedirectToLocal(string url)
        {
            if (Url.IsLocalUrl(url)) return Redirect(url);
            else return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}