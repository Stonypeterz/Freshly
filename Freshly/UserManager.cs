using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using Freshly.Identity.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Freshly.Identity
{
    public class UserManager : UserHelpers
    {
        private readonly IHttpContextAccessor HCA;
        private ApplicationUsersFactory DF { get; set; }

        public UserManager(IHttpContextAccessor _hca)
        {
            HCA = _hca;
            DF = new ApplicationUsersFactory();
        }

        public Task<ResultStatus> CreateAsync(ApplicationUser usr, string password)
        {
            var f = CreateUser(usr, password);
            return Task.FromResult(f);
        }

        public Task<UsersPage> GetUsersAsync(int pageno, int pagesize = 20, string searchQ = null)
        {
            return GetUsersAsync(null, pageno, pagesize, searchQ);
        }

        public Task<UsersPage> GetUsersAsync(string groupname, int pageno, int pagesize = 20, string searchQ = null)
        {
            if (pageno == 0) pageno = 1;
            if (pagesize == 0) pagesize = 20;
            var lst = DF.GetPage(groupname, pageno, pagesize, searchQ);
            return Task.FromResult(lst);
        }

        public Task<ResultStatus> ChangePasswordAsync(string UserId, string OldPassword, string NewPassword)
        {
            ApplicationUser usr;
            Status LR;
            var res = ValidatePassword(NewPassword);
            if (res.IsSuccessful)
            {
                var tup = ValidateUser(UserId);
                LR = tup.lr;
                usr = tup.usr;
                if (LR == Status.Success)
                {
                    var f = ComparePasswords(usr, OldPassword);
                    if (f)
                    {
                        usr.Password = NewPassword.ToHashed();
                        DF.Update(usr);
                        return Task.FromResult(new ResultStatus(true, GetMessage(Status.Success)));
                    }
                    else return Task.FromResult(new ResultStatus(GetMessage(Status.WrongPassword)));
                }
                else return Task.FromResult(new ResultStatus(GetMessage(LR)));
            }
            return Task.FromResult(new ResultStatus(GetMessage(Status.InvalidNewPassword)));
        }

        public Task<ApplicationUser> GetUserAsync(string IdOrEmail)
        {
            var usr = DF.GetUser(IdOrEmail);
            return Task.FromResult(usr);
        }

        public Task<string> GenerateResetPasswordCodeAsync(string userid)
        {
            var ecm = AllConsts.RPR.Find(e => e.UserId.ToLower() == userid.ToLower());
            if (ecm != null) AllConsts.RPR.Remove(ecm);
            var cm = new CodeModel(userid);
            AllConsts.RPR.Add(cm);
            return Task.FromResult(cm.Code);
        }

        public Task<ResultStatus> ResetPasswordAsync(string securityCode, string NewPassword)
        {
            var cm = AllConsts.RPR.Find(c => c.Code == securityCode && c.Expires > DateTime.UtcNow);
            if (cm == null) return Task.FromResult(new ResultStatus(GetMessage(Status.InvalidCodeOrId)));
            var res = ValidatePassword(NewPassword);
            if (res.IsSuccessful)
            {
                var usr = DF.GetUser(cm.UserId);
                if (usr?.CurrentStatus == AccountStatus.Active)
                {
                    usr.Password = NewPassword.ToHashed();
                    DF.Update(usr);
                    return Task.FromResult(new ResultStatus(true,"Successful"));
                }
                else return Task.FromResult(new ResultStatus($"Error validating the user account: The account is currently {usr?.CurrentStatus}."));
            }
            return Task.FromResult(res);
        }

        public async Task<ResultStatus> SignInAsync(string userid, string password)
        {
            return await SignInAsync(userid, password, false);
        }

        public async Task<ResultStatus> SignInAsync(string userid, string password, bool persist)
        {
            var context = HCA.HttpContext;
            var tup = ValidateUser(userid);
            var usr = tup.usr;
            var rslt = tup.lr;
            if (rslt == Status.Success)
            {
                if (!ComparePasswords(usr, password))
                {
                    usr.IsLogedIn = false;
                    rslt = Status.WrongPassword;
                    if (usr?.CurrentStatus == AccountStatus.Active)
                    {
                        var atp = AllConsts.FPA.Find(a => a.UserId == usr?.UserId);
                        var an = new AttemptModel(usr?.UserId);
                        if (atp != null)
                        {
                            an.FaildAttempts += atp.FaildAttempts;
                            AllConsts.FPA.Remove(atp);
                        }
                        AllConsts.FPA.Add(an);

                        if (an.FaildAttempts >= Freshly.D.MaxLoginAttempts)
                        {
                            usr.CurrentStatus = AccountStatus.Locked;
                            usr.LastLockDate = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, usr.UserId),
                        new Claim(ClaimTypes.Name, usr.UserId),
                        new Claim(ClaimTypes.Role, usr.Groups),
                        new Claim(ClaimTypes.Email, usr.Email),
                        new Claim(ClaimTypes.MobilePhone, usr.PhoneNumber),
                        new Claim("given_name", usr.FirstName),
                        new Claim("family_name", usr.LastName),
                        new Claim("gender", usr.Gender)
                    };

                    if (!string.IsNullOrEmpty(usr.Groups))
                        claims.Add(new Claim(ClaimTypes.Role, usr.Groups));

                    var identity = new ClaimsIdentity(AllConsts.AuthenticationType);
                    identity.AddClaims(claims);
                    var principle = new ClaimsPrincipal(identity);

                    //await context.SignInAsync(principle);

                    await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    principle,
                    new AuthenticationProperties()
                    {
                        IsPersistent = persist
                    });
                    usr.IsLogedIn = true;
                }
                DF.Update(usr);
            }
            return new ResultStatus(rslt == Status.Success, GetMessage(rslt));
        }

        public async Task<string> GenerateTokenAsync(string userid, string password)
        {
            return await GenerateTokenAsync(userid, password, 
                Freshly.D.TokenIssuer, Freshly.D.TokenAudience, 
                Freshly.D.TokenKey);
        }

        public async Task<string> GenerateTokenAsync(string userid, string password, string authority, string audience, string seckey)
        {
            var user = await GetUserAsync(userid);

            if (user == null) throw new Exception("Invalid user id."); else
            {
                if (user.CurrentStatus != AccountStatus.Active)
                    throw new Exception($"This account is currently {user.CurrentStatus}.");
                if (ComparePasswords(user, password))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.UserId),
                        new Claim(ClaimTypes.Role, user.Groups),
                        new Claim(JwtRegisteredClaimNames.NameId, user.UserId),
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                        new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                        new Claim(JwtRegisteredClaimNames.Gender, user.Gender)
                    };

                    var tvp = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(seckey));
                    var creds = new SigningCredentials(tvp, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: authority,
                        audience: audience,
                        claims: claims,
                        notBefore: DateTime.UtcNow,
                        expires: DateTime.UtcNow.AddDays(30),
                        signingCredentials: creds);

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
                else
                {
                    throw new Exception("Wrong password.");
                }
            }
        }

        public Task<UserAssignedGroups> GetAssignedGroupsAsync(string UserId)
        {
            var gs = DF.GetGroups(UserId);
            return Task.FromResult(gs);
        }

        public Task SignOutAsync()
        {
            var context = HCA.HttpContext;
            DF.SetLogedIn(false, context?.User?.Identity?.Name);
            return context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Sets a user account status to Active, Locked, Deleted or whatever else makes sence to you.
        /// </summary>
        /// <param name="UserId">The User Account Identifier (Username, Email or Phone number)</param>
        /// <param name="status">The Status you want to change to</param>
        /// <returns></returns>
        public Task<bool> ChangeStatusAsync(string UserId, Status status)
        {
            bool f = DF.ChangeStatus(UserId, status.ToString());
            return Task.FromResult(f);
        }

        /// <summary>
        /// Sets a User Account to Available, Busy or whatever else makes sence to you
        /// </summary>
        /// <param name="UserId">The User Account Identifier (Username, Email or Phone number)</param>
        /// <param name="status">The Status you want to change to</param>
        /// <returns></returns>
        public Task<bool> SetOnlineStatusAsync(string UserId, string status)
        {
            var f = DF.SetOnlineStatus(UserId, status.ToString());
            return Task.FromResult(f);
        }

        public Task<ResultStatus> ActivateAsync(string UserId)
        {
            return ActivateAsync(DF.GetUserByID(UserId));
        }

        public Task<ResultStatus> ActivateAsync(ApplicationUser usr)
        {
            if(usr == null) return Task.FromResult(new ResultStatus("The user object referenced is null"));
            if (usr.CurrentStatus == AccountStatus.Pending) {
                usr.CurrentStatus = AccountStatus.Active;
                DF.Update(usr);
                return Task.FromResult(new ResultStatus(true, "Successful"));
            } else {
                return Task.FromResult(new ResultStatus("Sorry, this account had already been activated. You can no longer use the activation link."));
            }
        }

        public Task<ResultStatus> UpdateAsync(ApplicationUser usr)
        {
            if (usr == null) return Task.FromResult(new ResultStatus("The user object referenced is null"));
            usr.IsLogedIn = false;
            DF.Update(usr);
            return Task.FromResult(new ResultStatus(true, "Successful"));
        }

        public Task<bool> AddUserToGroupAsync(string UserId, string GroupName)
        {
            using (var ug = new UserGroupsFactory())
            {
                ug.AddUserToGroup(UserId, GroupName);
            };
            return Task.FromResult(true);
        }

        public Task<bool> AddUserToGroupsAsync(string UserId, string[] GroupNames)
        {
            using (var ug = new UserGroupsFactory()) {
                foreach (var item in GroupNames) {
                    ug.AddUserToGroup(UserId, item);
                }
            };
            return Task.FromResult(true);
        }

        public Task<bool> RemoveUserFromGroupAsync(string UserId, string GroupName)
        {
            using (var ug = new UserGroupsFactory())
            {
                ug.RemoveUserFromGroup(UserId, GroupName);
            };
            return Task.FromResult(true);
        }

        public Task<bool> RemoveUserFromGroupsAsync(string UserId, string[] GroupNames)
        {
            using (var ug = new UserGroupsFactory())
            {
                foreach (var item in GroupNames)
                {
                    ug.RemoveUserFromGroup(UserId, item);
                }
            };
            return Task.FromResult(true);
        }

        public Task<bool> RemoveUserFromAllGroupsAsync(string UserId, string[] GroupNames)
        {
            using (var ug = new UserGroupsFactory())
            {
                ug.ClearGroups(UserId);
            };
            return Task.FromResult(true);
        }

        public Task<UserAvatar> GetAvatarAsync(string UserId)
        {
            UserAvatar uavt;
            using (var avt = new UserAvatarsFactory()) {
                uavt = avt.GetAvatar(UserId);
            }
            return Task.FromResult(uavt);
        }

        public Task UpdateAvatarAsync(UserAvatar userAvt)
        {
            using (var avt = new UserAvatarsFactory()) {
                var uavt = avt.GetAvatar(userAvt.UserId);
                if (uavt?.IsDefault == true) avt.Insert(userAvt);
                else avt.Update(userAvt);
            }
            return Task.CompletedTask;
        }

        public Task<ResultStatus> DeleteAsync(string id)
        {
            var r = new ResultStatus();
            using (var df = new ApplicationUsersFactory()) {
                var i = df.Delete(id);
                r.IsSuccessful = i > 0;
                if (r.IsSuccessful) r.Message = "The record was successfully deleted.";
            }
            return Task.FromResult(r);
        }
    }
}
