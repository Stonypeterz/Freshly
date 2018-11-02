using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Freshly.Identity;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Freshly.UI.Controllers
{
    public class GroupsController : Controller
    {
        private readonly GroupManager groupManager;

        public GroupsController(GroupManager gm)
        {
            groupManager = gm;
        }

    }
}
