using Freshly.Identity;
using Freshly.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.ViewComponents
{
    public class ProfileTabsViewComponent : ViewComponent 
    {
        private UserManager userManager;
        public ProfileTabsViewComponent(UserManager _usrMgr)
        {
            userManager = _usrMgr;
        }

        public async Task<IViewComponentResult> InvokeAsync(string UserId, string Tab)
        {
            switch (Tab.ToLower()) {
                case (PTNames.Groups):
                    var gs = await userManager.GetAssignedGroupsAsync(UserId);
                    return View("Groups", gs);
                case (PTNames.Edit):
                    var usr = await userManager.GetUserAsync(UserId);
                    var model = new UserUpdateModel() {
                        UserId = usr.UserId,
                        FirstName = usr.FirstName,
                        LastName = usr.LastName,
                        Email = usr.Email,
                        PhoneNumber = usr.PhoneNumber
                    };
                    return View("Edit", model);
                default:
                    return View("Resp", new ResponseObject(Alerts.danger, "No view", "We do not have any view for the tab name you entered."));
            }
        }
    }
}
