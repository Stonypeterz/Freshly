using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Freshly.Identity;
using Microsoft.AspNetCore.Authorization;
using Freshly.UI.Models;
using Freshly.Identity.Models;

namespace Freshly.UI.Controllers
{
    public class SecurityController : Controller
    {
        private readonly UserManager userManager;
        private readonly ICustomHelpers AppUtil;

        public SecurityController(UserManager _usrMgr, ICustomHelpers _appUtil)
        {
            userManager = _usrMgr;
            AppUtil = _appUtil;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DeniedAccess() => View("Resp", new ResponseObject() { Title = "Access Denied", Msg = AppUtil.GetMessage(MsgTypes.AccessDenied)});

        #region Helpers

        public IActionResult RedirectToLocal(string url)
        {
            if (Url.IsLocalUrl(url)) return Redirect(url);
            else return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}